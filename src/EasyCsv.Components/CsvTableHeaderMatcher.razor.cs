using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;
using FuzzySharp;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace EasyCsv.Components;

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


public partial class CsvTableHeaderMatcher
{
    [Inject] private ISnackbar? Snackbar { get; set; }
    [Inject] private ILogger<CsvTableHeaderMatcher>? Logger { get; set; }

    private List<ExpectedHeader>? _expectedHeaders;
    /// <summary>
    /// These headers that users match their csv headers to.
    /// </summary>
    [Parameter]
    public List<ExpectedHeader>? ExpectedHeaders
    {
        get => _expectedHeaders;
        set
        {
            if (_expectedHeaders == null)
            {
                _expectedHeaders = value;
                return;
            }

            // Only want backing to change when the expected headers actually change. Without this,
            // we lose the ordering applies to the expected headers when the parameters get set even
            // if it's the same expected headers.
            if (value != null && (value.Count != _expectedHeaders.Count || _expectedHeaders.Any(x => value.All(y => y.Equals(x) == false))))
            {
                Reset();
                _expectedHeaders = value.Select(x => x.DeepClone()).ToList();
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
    [Parameter] public List<ClassMap>? ClassMaps { get; set; }

    /// <summary>
    /// CsvHelper class maps to control how properties and fields in a class get converted from the csv.
    /// </summary>
    [Parameter] public List<ClassMap>? TypeConverters { get; set; }

    /// <summary>
    /// Controls how csv headers are automatically mapped to expected headers.
    /// </summary>
    [Parameter] public AutoMatching AutoMatch { get; set; } = AutoMatching.Strict;

    /// <summary>
    /// The EasyCsvConfiguration to use when <see cref="GetRecords"/> is called
    /// </summary>
    [Parameter]
    public EasyCsvConfiguration EasyCsvConfig { get; set; } = new ()
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
        if (_expectedHeaders == null) return;
        if (Csv == null || (_cachedCsv != null && _cachedCsv == Csv)) return;
        if (Csv?.GetColumns() == null) return;
        Reset();
        _cachedCsv = Csv;
        foreach (var header in Csv.GetColumns()!)
        {
            _originalHeaderCurrentHeaderDict[header] = header;
            _mappedDict[header] = null;
        }
        await MatchFileHeadersWithExpectedHeaders();
        _expectedHeaders = _expectedHeaders
            .OrderByDescending(x => x.Config.IsRequired)
            .ThenByDescending(x => _mappedDict.ContainsValue(x)).ToList();
        StateHasChanged();
    }

    public void Reset()
    {
        ExpectedHeaders?.ForEach(x =>
        {
            x.Value = null;
        });
        _mappedDict = new();
        _originalHeaderCurrentHeaderDict = new();
    }
    private async Task MatchFileHeadersWithExpectedHeaders()
    {
        var headers = Csv!.GetColumns();
        if (headers == null) return;
        List<ExpectedHeader> matchedHeaders = new ();
        foreach (var header in headers)
        {
            if (DoMatching(header, matchedHeaders, out var matchedHeader))
            {
                matchedHeaders.Add(matchedHeader!);
                await ReplaceColumn(header, matchedHeader!);
            }
        }
        AllHeadersValid = ValidateRequiredHeaders();
    }

    public async Task ReplaceColumn(string originalHeaderName, ExpectedHeader expectedHeader)
    {
        if (Csv == null) return;
        var currentHeaderName = _originalHeaderCurrentHeaderDict[originalHeaderName];
        await Csv.MutateAsync(x => x.ReplaceColumn(currentHeaderName, expectedHeader.CSharpPropertyName));
        _originalHeaderCurrentHeaderDict[originalHeaderName] = expectedHeader.CSharpPropertyName;
        _mappedDict[originalHeaderName] = expectedHeader;
        expectedHeader.Value = null;
        AllHeadersValid = ValidateRequiredHeaders();
    }

    public async Task ResetColumn(string originalHeaderName)
    {
        if (Csv == null) return;
        var currentHeaderName = _originalHeaderCurrentHeaderDict[originalHeaderName];
        await Csv.MutateAsync(x => x.ReplaceColumn(currentHeaderName, originalHeaderName));
        _originalHeaderCurrentHeaderDict[originalHeaderName] = originalHeaderName;
        _mappedDict[originalHeaderName] = null;
        AllHeadersValid = ValidateRequiredHeaders();
    }

    private bool DoMatching(string header, List<ExpectedHeader> ignore, out ExpectedHeader? matchedExpectedHeader)
    {
        matchedExpectedHeader = null;
        var matching = AutoMatching.Exact;
        do
        {
            var result = matching switch
            {
                AutoMatching.Exact => TryExactMatch(header, ignore, out matchedExpectedHeader),
                AutoMatching.Strict => TryFuzzyMatch(header, ignore, matching, out matchedExpectedHeader),
                AutoMatching.Lenient => TryFuzzyMatch(header, ignore, matching, out matchedExpectedHeader),
                _ => throw new NotImplementedException("AutMatching case not implemented")
            };
            if (result) return true;
            matching += 1;
        } while (matching <= AutoMatch);
        return false;
    }
    private bool TryExactMatch(string header, List<ExpectedHeader> ignore, out ExpectedHeader? matchedExpectedHeader)
    {
        var filteredHeaders = ExpectedHeaders?.Except(ignore);
        foreach (var expectedHeader in filteredHeaders ?? Enumerable.Empty<ExpectedHeader>())
        {
            foreach (var possibleExpectedHeader in expectedHeader.ValuesToMatch)
            {
                if (string.Compare(possibleExpectedHeader, header, StringComparison.CurrentCultureIgnoreCase) != 0) continue;
                matchedExpectedHeader = expectedHeader;
                return true;
            }
        }
        matchedExpectedHeader = null;
        return false;
    }
    private bool TryFuzzyMatch(string header, List<ExpectedHeader> ignore, AutoMatching matching, out ExpectedHeader? matchedExpectedHeader)
    {
        matchedExpectedHeader = null;
        var filteredHeaders = ExpectedHeaders?.Except(ignore).Where(x => x.Config.AutoMatching == null || x.Config.AutoMatching >= matching);
        if (filteredHeaders == null) return false;
        foreach (var expectedHeader in filteredHeaders)
        {
            foreach (var possibleExpectedHeader in expectedHeader.ValuesToMatch)
            {
                var ratio = Fuzz.Ratio(possibleExpectedHeader.ToLower(), header.ToLower());
                var partialRatio = Fuzz.PartialRatio(possibleExpectedHeader.ToLower(), header.ToLower());
                if (ratio > 90 || (matching == AutoMatching.Lenient && ratio > 60 && partialRatio > 90))
                {
                    matchedExpectedHeader = expectedHeader;
                    return true;
                }
            }
        }

        return false;
    }


    /// <summary>
    /// Attempt to convert csv to records 
    /// </summary>
    /// <typeparam name="T">The class that csv records will be read into</typeparam>
    /// <returns>A list containing the </returns>
    public async Task<List<T>?> GetRecords<T>()
    {
        if (!ValidateRequiredHeaders() || Csv == null)
        {
            return null;
        }
        try
        {
            await AddDefaultValues();

            if (typeof(T) != typeof(object))
            {
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
                    return await Csv.GetRecordsAsync<T>(EasyCsvConfig.CsvHelperConfig, csvContextProfile);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error getting records. CsvTableHeaderMatcher");
                    return null;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Snackbar?.Add("Something went wrong reading the CSV. Reset and try again.", Severity.Warning);
            Logger?.LogError(ex, "CsvTableHeaderMatcher of type {Type}, exception will getting records.", typeof(T).Name);
            return null;
        }
    }
    private Task AddDefaultValues()
    {
        if (ExpectedHeaders == null || Csv == null)
        {
            return Task.CompletedTask;
        }
        var defaultValueKvps = ExpectedHeaders.Where(x => x is {Config.DefaultValueType: not DefaultValueType.None, HasValue: true} or { Config.DefaultValueRenderFragment: not null, HasValue: true })
            .ToDictionary(x => x.CSharpPropertyName, x => x.Value);
        return Csv.MutateAsync(x => x.AddColumns(defaultValueKvps));
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
