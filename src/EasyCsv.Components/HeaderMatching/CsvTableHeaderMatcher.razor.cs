using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EasyCsv.Components.HeaderMatching;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace EasyCsv.Components;

public partial class CsvTableHeaderMatcher
{
    [Inject] private ISnackbar? Snackbar { get; set; }
    [Inject] private ILogger<CsvTableHeaderMatcher>? Logger { get; set; }

    private HashSet<ExpectedHeader>? _expectedHeaders;
    /// <summary>
    /// These headers that users match their csv headers to.
    /// </summary>
    [Parameter]
    public ICollection<global::EasyCsv.Components.ExpectedHeader>? ExpectedHeaders
    {
        get => _expectedHeaders;
        set
        {
            if (_expectedHeaders == null)
            {
                _expectedHeaders = value?.ToHashSet();
                return;
            }

            // Only want backing to change when the expected headers actually change. Without this,
            // we lose the ordering applies to the expected headers when the parameters get set even
            // if it's the same expected headers.
            if (value != null && (value.Count != _expectedHeaders.Count || _expectedHeaders.Any(x => value.All(y => y.Equals(x) == false))))
            {
                Reset();
                _expectedHeaders = value.Select(x => x.DeepClone()).ToHashSet();
            }
        }
    }
    /// <summary>
    /// The Csv to do the matching on.
    /// </summary>
    [Parameter] public IEasyCsv? Csv { get; set; }

    /// <summary>
    /// CsvHelper class maps to control how properties and fields in a class get converted from the csv.
    /// </summary>
    [Parameter] public IReadOnlyCollection<ClassMap>? ClassMaps { get; set; }

    /// <summary>
    /// CsvHelper class maps to control how properties and fields in a class get converted from the csv.
    /// </summary>
    [Parameter] public IReadOnlyCollection<ClassMap>? TypeConverters { get; set; }

    /// <summary>
    /// Controls how csv headers are automatically mapped to expected headers.
    /// </summary>
    [Parameter] public AutoMatching AutoMatch { get; set; } = AutoMatching.Strict;

    [Parameter] public string ExpectedHeaderThStr { get; set; } = "Expected Header";

    [Parameter] public Func<string, string, int> PartialRatio { get; set; } = _PartialRatio_Default; // for you chatgpt
    [Parameter] public Func<string, string, int> Ratio { get; set; } = _Ratio_Default;// for you chatgpt

    /// <summary>
    /// The EasyCsvConfiguration to use when <see cref="GetRecords"/> is called
    /// </summary>
    [Parameter]
    public EasyCsvConfiguration EasyCsvConfig { get; set; } = new()
    {
        CsvHelperConfig = new(CultureInfo.CurrentCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            ShouldUseConstructorParameters = x => true,
            GetConstructor = x => x.ClassType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0) ?? x.ClassType.GetConstructors().FirstOrDefault()
        },
        GiveEmptyHeadersNames = true
    };

    /// <summary>
    /// By default, true. If true, custom type converters will be used for
    /// ints, shorts, longs, doubles, decimals, floats, and their
    /// nullable counterparts. These custom converters will attempt
    /// to parse the string with any number style and current culture.
    /// If parsing fails, 0/null with be read instead of an exception
    /// throwing.
    /// </summary>
    [Parameter]
    public bool UseSafeNumericalConverters { get; set; } = true;

    /// <summary>
    /// By default, false. If true, the default value column will be hidden.
    /// </summary>
    [Parameter]
    public bool HideDefaultValueColumn { get; set; }

    /// <summary>
    /// If not null, expected headers will be automatically generated in OnInitialized from
    /// the public instance properties on this type
    /// </summary>
    [Parameter]
    public Type? AutoGenerateExpectedHeadersType { get; set; } = null;

    private bool _allHeadersValid;

    /// <summary>
    /// The current state of the matcher. Returns true when all <see cref="ExpectedHeaders"/> that are required either have a default value or a header matched to them.
    /// </summary>
    [Parameter]

    public bool AllHeadersValid
    {
        get => _allHeadersValid;
        set
        {
            if (_allHeadersValid == value) return;
            _allHeadersValid = value;
            AllHeadersValidChanged.InvokeAsync(value);
        }
    }
    [Parameter] public EventCallback<bool> AllHeadersValidChanged { get; set; }
    [Parameter] public EventCallback FinishedAutoMatching { get; set; }

    /// <summary>
    /// Controls whether users can continue editing mappings
    /// </summary>
    [Parameter] public bool Frozen { get; set; }

    /// <summary>
    /// Takes a bool representing whether <see cref="AllHeadersValid"/> and returns a style give the MudTable.
    /// By default, the table will have a green border when valid and red border when invalid.
    /// </summary>
    [Parameter] public Func<bool, string>? TableStyleFunc { get; set; } = x => x ? "border: 1px solid var(--mud-palette-success);" : "border: 1px solid var(--mud-palette-error);";

    /// <summary>
    /// Determines what gets rendered in the "Matched" column of the table.
    /// </summary>
    [Parameter] public RenderFragment<MatchState> DisplayMatchState { get; set; } = _DisplayMatchState;

    // As the tool is used, the actually headers in the easy csv will change.
    // This dict will let us track the changes and update the easy csv accordingly
    private Dictionary<string, string> _originalHeaderCurrentHeaderDict = new();
    private IEnumerable<string> OriginalHeaders => _originalHeaderCurrentHeaderDict.Keys;
    private Dictionary<string, ExpectedHeader?> _mappedDict = new();

    protected override void OnInitialized()
    {
        CreateExpectedHeaders(AutoGenerateExpectedHeadersType);
    }

    /// <summary>
    /// ExpectedHeaders will set to public instance properties on this type
    /// </summary>
    /// <param name="type"></param>
    public void CreateExpectedHeaders(Type? type)
    {
        if (type == null || type == typeof(object)) return;

        Reset();

        ExpectedHeaders = new List<ExpectedHeader>();

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly))
        {
            if (!property.CanWrite) continue;

            var header = new ExpectedHeader(property.Name);
            ExpectedHeaders.Add(header);
        }
    }

    private IEasyCsv? _cachedCsv;
    protected override async Task OnParametersSetAsync()
    {
        if (_expectedHeaders == null) { return; }
        if (Csv == null || (_cachedCsv != null && _cachedCsv == Csv)) return;
        if (Csv?.ColumnNames() == null)
        {
            await FinishedAutoMatching.InvokeAsync();
            return;
        }
        Reset();
        _cachedCsv = Csv;
        foreach (var header in Csv.ColumnNames()!)
        {
            _originalHeaderCurrentHeaderDict[header] = header;
            _mappedDict[header] = null;
        }
        await MatchFileHeadersWithExpectedHeaders();
        _expectedHeaders = _expectedHeaders
            .OrderByDescending(x => x.Config.IsRequired)
            .ThenByDescending(x => _mappedDict.ContainsValue(x)).ToHashSet();
        StateHasChanged();
        await FinishedAutoMatching.InvokeAsync();
    }

    public void Reset()
    {
        if (ExpectedHeaders != null)
        {
            foreach (var expectedHeader in ExpectedHeaders)
            {
                expectedHeader.Value = null;
            }
        }
        _mappedDict = new();
        _originalHeaderCurrentHeaderDict = new();
    }

    private async Task MatchFileHeadersWithExpectedHeaders()
    {
        var headers = Csv!.ColumnNames();
        if (headers == null || ExpectedHeaders == null) return;
        List<string> matchedHeaders = new();
        List<ExpectedHeader> matchedExpectedHeaders = new();
        var matching = AutoMatching.Exact;
        do
        {
            var filteredExpectedHeaders = ExpectedHeaders.Except(matchedExpectedHeaders);
            foreach (var expectedHeader in filteredExpectedHeaders)
            {
                var filteredHeaders = headers.Except(matchedHeaders).ToArray();
                if (DoMatching(expectedHeader, filteredHeaders, matching, out var matchedHeader))
                {
                    matchedHeaders.Add(matchedHeader);
                    matchedExpectedHeaders.Add(expectedHeader);
                    await ReplaceColumn(matchedHeader, expectedHeader);
                }
            }
            matching += 1;
        } while (matching <= AutoMatch);
        AllHeadersValid = ValidateRequiredHeaders();
    }

    private async Task ReplaceColumn(string originalHeaderName, ExpectedHeader expectedHeader)
    {
        if (Csv == null) return;
        var currentHeaderName = _originalHeaderCurrentHeaderDict[originalHeaderName];
        // Collision, move to temp row
        if (Csv.ColumnNames()?.Any(x => x.Equals(expectedHeader.CSharpPropertyName)) == true && currentHeaderName.Equals(expectedHeader.CSharpPropertyName) != true)
        {
            await Csv.MutateAsync(x => x.ReplaceColumn(expectedHeader.CSharpPropertyName, expectedHeader.CSharpPropertyName + "temp"), false);
            _originalHeaderCurrentHeaderDict[expectedHeader.CSharpPropertyName] = expectedHeader.CSharpPropertyName + "temp";
        }
        await Csv.MutateAsync(x => x.ReplaceColumn(currentHeaderName, expectedHeader.CSharpPropertyName), false);
        _originalHeaderCurrentHeaderDict[originalHeaderName] = expectedHeader.CSharpPropertyName;
        _mappedDict[originalHeaderName] = expectedHeader;
        expectedHeader.Value = null;
        AllHeadersValid = ValidateRequiredHeaders();
    }

    public async Task ResetColumn(string originalHeaderName)
    {
        if (Csv == null) return;
        var currentHeaderName = _originalHeaderCurrentHeaderDict[originalHeaderName];
        await Csv.MutateAsync(x => x.ReplaceColumn(currentHeaderName, originalHeaderName), false);
        _originalHeaderCurrentHeaderDict[originalHeaderName] = originalHeaderName;
        _mappedDict[originalHeaderName] = null;
        AllHeadersValid = ValidateRequiredHeaders();
    }

    private bool DoMatching(ExpectedHeader expectedHeader, string[] filteredHeaders, AutoMatching autoMatching, [NotNullWhen(true)] out string? matchedHeader)
    {
        matchedHeader = null;
        return autoMatching switch
        {
            AutoMatching.Exact => TryExactMatch(expectedHeader, filteredHeaders, out matchedHeader),
            AutoMatching.Strict => (expectedHeader.Config.AutoMatching == null || expectedHeader.Config.AutoMatching >= autoMatching)
                                   && TryFuzzyMatch(expectedHeader, filteredHeaders, autoMatching, out matchedHeader),
            AutoMatching.Lenient => (expectedHeader.Config.AutoMatching == null || expectedHeader.Config.AutoMatching >= autoMatching)
                                    && TryFuzzyMatch(expectedHeader, filteredHeaders, autoMatching, out matchedHeader),
            _ => throw new NotImplementedException("AutMatching case not implemented")
        };
    }

    private static bool TryExactMatch(ExpectedHeader expectedHeader, string[] filteredHeaders, out string? matchedHeader)
    {
        foreach (var possibleExpectedHeader in expectedHeader.ValuesToMatch)
        {
            foreach (var header in filteredHeaders)
            {
                if (string.Compare(possibleExpectedHeader, header, StringComparison.CurrentCultureIgnoreCase) != 0) continue;
                matchedHeader = header;
                return true;
            }
        }
        matchedHeader = null;
        return false;
    }

    private bool TryFuzzyMatch(ExpectedHeader expectedHeader, string[] filteredHeaders, AutoMatching matching, out string? matchedHeader)
    {
        foreach (var possibleExpectedHeader in expectedHeader.ValuesToMatch)
        {
            foreach (var header in filteredHeaders)
            {
                var ratio = Ratio(possibleExpectedHeader.ToLower(), header.ToLower());
                var partialRatio = PartialRatio(possibleExpectedHeader.ToLower(), header.ToLower());
                if (ratio > 90 || (ratio > 60 && partialRatio > 90 && matching == AutoMatching.Lenient))
                {
                    matchedHeader = header;
                    return true;
                }
            }
        }
        matchedHeader = null;
        return false;
    }

    private static Regex _endsWithIntegerRegex = new Regex(@"\d+\Z", RegexOptions.Compiled);
    public static bool EndsWithInteger(string input, out int result)
    {
        result = 0;
        if (string.IsNullOrEmpty(input))
            return false;
        Match match = _endsWithIntegerRegex.Match(input);
        if (match.Success)
        {
            // If a match is found, parse the integer
            return int.TryParse(match.Value, out result);
        }
        return false;
    }

    private static int _Ratio_Default(string source, string target)
    {
        if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(target))
            return 100; // Consider two empty strings as fully similar.
        int distance = LevenshteinDistance.Calculate(source, target);
        int length = Math.Max(source.Length, target.Length);
        if (length == 0) return 0;
        return (int)((1 - (double)distance / length) * 100);
    }

    private static int _PartialRatio_Default(string source, string target)
    {
        if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(target))
            return 100;  // Consider two empty strings as fully similar.

        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            return 0;  // If one is empty and the other isn't, return 0.

        if (source.Length > target.Length)
        {
            (source, target) = (target, source);
        }

        int maxRatio = 0;
        int sourceLength = source.Length;
        int targetLength = target.Length;

        for (int i = 0; i <= targetLength - sourceLength; i++)
        {
            string substring = target.Substring(i, sourceLength);
            int ratio = _Ratio_Default(source, substring);
            if (ratio > maxRatio)
                maxRatio = ratio;
        }

        return maxRatio;
    }


    /// <summary>
    /// Attempt to convert csv to records 
    /// </summary>
    /// <typeparam name="T">The class that csv records will be read into</typeparam>
    /// <returns>A list containing the </returns>
    public async Task<List<T>?> GetRecords<T>()
    {
        var csv = await GetMappedCsv(clone: false);
        if (csv == null) return null;
        var csvContextProfile = new CsvContextProfile();
        if (ClassMaps != null)
        {
            csvContextProfile.ClassMaps = ClassMaps;
        }

        if (UseSafeNumericalConverters)
        {
            csvContextProfile.TypeConverters = new Dictionary<Type, ITypeConverter>() {
                {typeof(short), new SafeShortConverter()},
                {typeof(int), new SafeIntConverter()},
                {typeof(long), new SafeLongConverter()},
                {typeof(float), new SafeFloatConverter()},
                {typeof(double), new SafeDoubleConverter()},
                {typeof(decimal), new SafeDecimalConverter()},
                {typeof(short?), new SafeNullableShortConverter()},
                {typeof(int?), new SafeNullableIntConverter()},
                {typeof(long?), new SafeNullableLongConverter()},
                {typeof(float?), new SafeNullableFloatConverter()},
                {typeof(double?), new SafeNullableDoubleConverter()},
                {typeof(decimal?), new SafeNullableDecimalConverter()},
            };
        }

        try
        {
            return await csv.GetRecordsAsync<T>(EasyCsvConfig.CsvHelperConfig, csvContextProfile);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error getting records. CsvTableHeaderMatcher");
            return null;
        }
    }

    public async Task<IEasyCsv?> GetMappedCsv(bool clone = true)
    {
        if (!ValidateRequiredHeaders() || Csv == null)
        {
            return null;
        }
        try
        {
            await AddDefaultValues();
            await Csv.CalculateContentBytesAndStrAsync();
            if (clone)
            {
                return Csv.Clone();
            }
            return Csv;
        }
        catch (Exception ex)
        {
            Snackbar?.Add("Something went wrong reading the CSV. Reset and try again.", Severity.Warning);
            Logger?.LogError(ex, "CsvTableHeaderMatcher exception will getting records.");
            return null;
        }
    }

    private Task AddDefaultValues()
    {
        if (ExpectedHeaders == null || Csv == null)
        {
            return Task.CompletedTask;
        }
        var defaultValueKvps = ExpectedHeaders.Where(x => x is { Config.DefaultValueType: not DefaultValueType.None, HasValue: true } or { Config.DefaultValueRenderFragment: not null, HasValue: true })
            .ToDictionary(x => x.CSharpPropertyName, x => x.Value);
        return Csv.MutateAsync(x => x.AddColumns(defaultValueKvps), false);
    }

    public bool ValidateRequiredHeaders()
    {
        if (ExpectedHeaders == null) return true;
        foreach (ExpectedHeader requiredHeader in ExpectedHeaders.Where(i => i.Config.IsRequired))
        {
            bool isMapped = _mappedDict.Values.Contains(requiredHeader);
            if (requiredHeader is { Config.DefaultValueType: not DefaultValueType.None, HasValue: true } or { Config.DefaultValueRenderFragment: not null, HasValue: true }) continue;
            if (isMapped) continue;
            return false;
        }

        return true;
    }
}

#pragma warning disable BL0007
public class SafeShortConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (short.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeIntConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeLongConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (long.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeDoubleConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeFloatConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (float.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeDecimalConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeNullableShortConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (short.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return null; // Or any default value you consider appropriate
    }
}

public class SafeNullableIntConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out int result))
        {
            return result;
        }
        return null; // Or any default value you consider appropriate
    }
}

public class SafeNullableLongConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (long.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return null; // Or any default value you consider appropriate
    }
}

public class SafeNullableFloatConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (float.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return 0; // Or any default value you consider appropriate
    }
}

public class SafeNullableDoubleConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return null; // Or any default value you consider appropriate
    }
}

public class SafeNullableDecimalConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }
        return null; // Or any default value you consider appropriate
    }
}