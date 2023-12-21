using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace EasyCsv.Components;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ExpectedHeader
{
    private string DebuggerDisplay => $"CSharpPropName: {CSharpPropertyName}, DisplayName: {DisplayName}, Required: {Config.IsRequired}";

    /// <summary>
    /// This should be the name of the property/field that you want the column to map to when reading
    /// the csv into objects.
    /// </summary>
    internal string CSharpPropertyName { get; init; }

    /// <summary>
    /// AutoMatching will be done on all the values in this list
    /// </summary>
    internal List<string> ValuesToMatch { get; init; }

    internal string DisplayName { get; init; }

    /// <summary>
    /// AutoMatching will be done on all the values in this list
    /// </summary>
    internal ExpectedHeaderConfig Config { get; init; }

    internal object? Value { get; set; }
    internal bool HasValue => Value is string str ? !string.IsNullOrWhiteSpace(str) : Value != null;

    /// <summary>
    /// Expected headers represents csharp properties that you want a value for when reading a csv.
    /// Users can map a header from the csv or provide a default value or neither.
    /// </summary>
    /// <param name="csharpPropertyName">Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher{T}.GetRecords"/> is called.</param>
    /// <param name="displayName">
    /// The value that will be displayed in the 'Expected Header' column of the matcher. If this matches <paramref name="csharpPropertyName"/> it will be split
    /// on capital letters. E.g. "FirstName" will be displayed as "First Name"
    /// </param>
    /// <param name="valuesToMatch">The header values this expected header should match with when a csv is uploaded.</param>
    /// <param name="config">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, string displayName, List<string> valuesToMatch, ExpectedHeaderConfig? config = null)
    {
        ValuesToMatch = valuesToMatch;
        CSharpPropertyName = csharpPropertyName;
        DisplayName = displayName;
        if (config == null) config = ExpectedHeaderConfig.Default;
        Config = config;
        Value = config.InitialDefaultValue;
    }

    /// <summary>
    /// Expected headers represents csharp properties that you want a value for when reading a csv.
    /// Users can map a header from the csv or provide a default value or neither.
    /// </summary>
    /// <param name="csharpPropertyName">Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher{T}.GetRecords"/> is called.</param>
    /// <param name="displayName">
    /// The value that will be displayed in the 'Expected Header' column of the matcher. If this matches <paramref name="csharpPropertyName"/> it will be split
    /// on capital letters. E.g. "FirstName" will be displayed as "First Name"
    /// </param>
    /// <param name="valuesToMatch">The header values this expected header should match with when a csv is uploaded.</param>
    /// <param name="configurator">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, string displayName, List<string> valuesToMatch, Action<ExpectedHeaderConfigurator>? configurator = null)
    {
        ValuesToMatch = valuesToMatch;
        CSharpPropertyName = csharpPropertyName;
        DisplayName = displayName;
        if (configurator != null)
        {
            var expectedHeaderConfigurator = new ExpectedHeaderConfigurator();
            configurator(expectedHeaderConfigurator);
            var config = new ExpectedHeaderConfig(expectedHeaderConfigurator.IsRequired, expectedHeaderConfigurator.DefaultValueType, expectedHeaderConfigurator.DefaultValueRenderFragment, expectedHeaderConfigurator.InitialDefaultValue);
            Config = config;
        }
        else
        {
            Config = ExpectedHeaderConfig.Default;
        }
        Value = Config.InitialDefaultValue;
    }

    /// <summary>
    /// Expected headers represents csharp properties that you want a value for when reading a csv.
    /// Users can map a header from the csv or provide a default value or neither.
    /// </summary>
    /// <param name="csharpPropertyName">
    /// Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher{T}.GetRecords"/> is called.
    /// <paramref name="csharpPropertyName"/> will also be used as the display name which was what's displayed in the 'Expected Header' column of the matcher,
    /// but it's value will be split on capital letters. E.g. "FirstName" will be displayed as "First Name".
    /// <paramref name="csharpPropertyName"/> will also be the only item in the <see cref="ValuesToMatch"/> list which is used to match csv headers to this.
    /// </param>
    /// <param name="config">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, ExpectedHeaderConfig? config = null) : this(csharpPropertyName,
        csharpPropertyName, [csharpPropertyName], config)
    {
        
    }


    /// <summary>
    /// Expected headers represents csharp properties that you want a value for when reading a csv.
    /// Users can map a header from the csv or provide a default value or neither.
    /// </summary>
    /// <param name="csharpPropertyName">
    /// Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher{T}.GetRecords"/> is called.
    /// <paramref name="csharpPropertyName"/> will also be used as the display name which was what's displayed in the 'Expected Header' column of the matcher,
    /// but it's value will be split on capital letters. E.g. "FirstName" will be displayed as "First Name".
    /// <paramref name="csharpPropertyName"/> will also be the only item in the <see cref="ValuesToMatch"/> list which is used to match csv headers to this.
    /// </param>
    /// <param name="configurator">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, Action<ExpectedHeaderConfigurator> configurator) : this(csharpPropertyName,
        csharpPropertyName, [csharpPropertyName], configurator)
    {

    }
}
public class ExpectedHeaderConfig
{
    internal ExpectedHeaderConfig(bool required, DefaultValueType allowDefaultValue,
        RenderFragment<DefaultValueRenderFragmentsArgs>? defaultValueRenderFragment, object? initialDefaultValue)
    {
        IsRequired = required;
        DefaultValueType = allowDefaultValue;
        DefaultValueRenderFragment = defaultValueRenderFragment;
        InitialDefaultValue = initialDefaultValue;
    }

    public ExpectedHeaderConfig(DefaultValueType defaultValueType = DefaultValueType.None, bool required = false, object? initialDefaultValue = null) :
        this(required, defaultValueType, null, initialDefaultValue) { }

    public ExpectedHeaderConfig(RenderFragment<DefaultValueRenderFragmentsArgs>? defaultValueRenderFragment, bool required = false, object? initialDefaultValue = null) :
        this(required, DefaultValueType.None, defaultValueRenderFragment, initialDefaultValue)
    {
        
    }

    /// <summary>
    /// If required, a default value must be provided or a header must be matched to the expected header with this config for the matcher to be considered valid.
    /// </summary>
    public bool IsRequired { get; private init; }
    /// <summary>
    /// Determines whether or
    /// </summary>
    public DefaultValueType DefaultValueType { get; private init; }
    public object? InitialDefaultValue { get; set; }
    public RenderFragment<DefaultValueRenderFragmentsArgs>? DefaultValueRenderFragment { get; private init; }
    /// <summary>
    /// A default value will NOT be allowed. A csv header can be mapped but is not required for the matcher to be considered valid. 
    /// </summary>
    public static ExpectedHeaderConfig Default = new ExpectedHeaderConfig();
    /// <summary>
    ///  A default value will NOT be allowed. A csv header must be mapped or the matcher will be considered invalid.
    /// </summary>
    public static ExpectedHeaderConfig Required = new ExpectedHeaderConfig(required: true);
    /// <summary>
    /// A default value with a text input will be allowed. A csv header can also be mapped but neither are required for the matcher to be considered valid. 
    /// </summary>
    public static ExpectedHeaderConfig TextDefaultValue = new ExpectedHeaderConfig(defaultValueType: DefaultValueType.Text);
    /// <summary>
    /// A default value with a text input will be allowed. A csv header can also be mapped, one of either are required for the matcher to be considered valid. 
    /// </summary>
    public static ExpectedHeaderConfig RequiredTextDefaultValue = new ExpectedHeaderConfig(required: true, defaultValueType: DefaultValueType.None);
}

public class ExpectedHeaderConfigurator
{
    public bool IsRequired { get; set; }
    public DefaultValueType DefaultValueType { get; set; }
    public RenderFragment<DefaultValueRenderFragmentsArgs>? DefaultValueRenderFragment { get; set; }
    public object? InitialDefaultValue { get; set; }
}

public class DefaultValueRenderFragmentsArgs(bool frozen, Action<object?> defaultValueChanged)
{
    public bool Frozen { get; init; } = frozen;
    public Action<object?> DefaultValueChanged { get; init; } = defaultValueChanged;
};