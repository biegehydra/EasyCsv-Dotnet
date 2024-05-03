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
    internal string[] ValuesToMatch { get; private set; }

    internal string DisplayName { get; init; }
    internal ExpectedHeaderConfig Config { get; private set; }
    internal object? Value { get; set; }
    internal bool HasValue => Value is string str ? !string.IsNullOrWhiteSpace(str) : Value != null;

    /// <summary>
    /// Expected headers represents csharp properties that you want a value for when reading a csv.
    /// Users can map a header from the csv or provide a default value or neither.
    /// </summary>
    /// <param name="csharpPropertyName">Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher.GetRecords"/> is called.</param>
    /// <param name="displayName">
    /// The value that will be displayed in the 'Expected Header' column of the matcher. If this matches <paramref name="csharpPropertyName"/> it will be split
    /// on capital letters. E.g. "FirstName" will be displayed as "First Name"
    /// </param>
    /// <param name="valuesToMatch">The header values this expected header should match with when a csv is uploaded.</param>
    /// <param name="config">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, string displayName, ICollection<string> valuesToMatch, ExpectedHeaderConfig? config = null)
    {
        ValuesToMatch = valuesToMatch.ToArray();
        CSharpPropertyName = csharpPropertyName;
        DisplayName = displayName;
        config ??= ExpectedHeaderConfig.Default;
        Config = config;
        Value = config.InitialDefaultValue;
    }

    /// <summary>
    /// Expected headers represents csharp properties that you want a value for when reading a csv.
    /// Users can map a header from the csv or provide a default value or neither.
    /// </summary>
    /// <param name="csharpPropertyName">Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher.GetRecords"/> is called.</param>
    /// <param name="displayName">
    /// The value that will be displayed in the 'Expected Header' column of the matcher. If this matches <paramref name="csharpPropertyName"/> it will be split
    /// on capital letters. E.g. "FirstName" will be displayed as "First Name"
    /// </param>
    /// <param name="valuesToMatch">The header values this expected header should match with when a csv is uploaded.</param>
    /// <param name="configurator">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, string displayName, ICollection<string> valuesToMatch, Action<ExpectedHeaderConfigurator>? configurator = null)
    {
        ValuesToMatch = valuesToMatch.ToArray();
        CSharpPropertyName = csharpPropertyName;
        DisplayName = displayName;
        if (configurator != null)
        {
            var expectedHeaderConfigurator = new ExpectedHeaderConfigurator();
            configurator(expectedHeaderConfigurator);
            var config = new ExpectedHeaderConfig(expectedHeaderConfigurator.IsRequired, expectedHeaderConfigurator.DefaultValueType,
                expectedHeaderConfigurator.DefaultValueRenderFragment, expectedHeaderConfigurator.InitialDefaultValue, expectedHeaderConfigurator.AutoMatching);
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
    /// Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher.GetRecords"/> is called.
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
    /// Values from the csv column this is matched to, or from the default value provided, will be put into this property when <see cref="CsvTableHeaderMatcher.GetRecords"/> is called.
    /// <paramref name="csharpPropertyName"/> will also be used as the display name which was what's displayed in the 'Expected Header' column of the matcher,
    /// but it's value will be split on capital letters. E.g. "FirstName" will be displayed as "First Name".
    /// <paramref name="csharpPropertyName"/> will also be the only item in the <see cref="ValuesToMatch"/> list which is used to match csv headers to this.
    /// </param>
    /// <param name="configurator">Configurations for the expected header</param>
    public ExpectedHeader(string csharpPropertyName, Action<ExpectedHeaderConfigurator> configurator) : this(csharpPropertyName,
        csharpPropertyName, [csharpPropertyName], configurator)
    {

    }

    public bool Equals(ExpectedHeader? other)
    {
        if (other == null) return false;
        return CSharpPropertyName == other.CSharpPropertyName && DisplayName == other.DisplayName &&
               ValuesToMatch.Length == other.ValuesToMatch.Length && ValuesToMatch.All(other.ValuesToMatch.Contains) 
               && Config.Equals(other.Config);
    }

    public ExpectedHeader DeepClone()
    {
        var clone = (ExpectedHeader) MemberwiseClone();
        clone.Config = Config.Clone();
        clone.ValuesToMatch = ValuesToMatch.ToArray();
        return clone;
    }
}
public class ExpectedHeaderConfig
{
    internal ExpectedHeaderConfig(bool required, DefaultValueType defaultValueType,
        RenderFragment<DefaultValueRenderFragmentArgs>? defaultValueRenderFragment, object? initialDefaultValue, AutoMatching? autoMatching)
    {
        IsRequired = required;
        DefaultValueType = defaultValueType;
        DefaultValueRenderFragment = defaultValueRenderFragment;
        InitialDefaultValue = initialDefaultValue;
        AutoMatching = autoMatching;
    }

    /// <param name="defaultValueType">
    /// Options are None, Text, DateTime, CheckBox, and TriStateCheckBox. These will control what MudBlazor input component is used in the default value column.
    /// This value is ignored if a value is provided for DefaultValueRenderFragment</param>
    /// <param name="required">
    /// All EHs that are marked as required must either have a default value provided or csv header mapped to be marked as valid. If any EH is invalid,
    /// it will show up as red in the table and the whole table will have a red border (configurable).
    /// </param>
    /// <param name="initialDefaultValue">
    /// The initial default value of this expected header. Will show up in whatever input type is used in the "Default Value" column.
    /// </param>
    /// <param name="autoMatching">
    /// Set the auto matching level for this specific property
    /// </param>
    public ExpectedHeaderConfig(DefaultValueType defaultValueType = DefaultValueType.None, bool required = false, object? initialDefaultValue = null, AutoMatching? autoMatching = null) :
        this(required, defaultValueType, null, initialDefaultValue, autoMatching) { }

    /// <param name="defaultValueRenderFragment">
    /// A custom element that will be used in the "Default Value" column. This will override the DefaultValueType.
    /// </param>
    /// <param name="required">
    /// All EHs that are marked as required must either have a default value provided or csv header mapped to be marked as valid. If any EH is invalid,
    /// it will show up as red in the table and the whole table will have a red border (configurable).
    /// </param>
    /// <param name="initialDefaultValue">
    /// The initial default value of this expected header. Will show up in whatever input type is used in the "Default Value" column.
    /// </param>
    /// <param name="autoMatching">
    /// Set the auto matching level for this specific property
    /// </param>
    public ExpectedHeaderConfig(RenderFragment<DefaultValueRenderFragmentArgs>? defaultValueRenderFragment, bool required = false, object? initialDefaultValue = null, AutoMatching? autoMatching = null) :
        this(required, DefaultValueType.None, defaultValueRenderFragment, initialDefaultValue, autoMatching)
    {
        
    }

    /// <summary>
    /// Sets the auto matching level for this specific property
    /// </summary>
    public AutoMatching? AutoMatching { get; private init; }

    /// <summary>
    /// If required, a default value must be provided or a header must be matched to the expected header with this config for the matcher to be considered valid.
    /// </summary>
    public bool IsRequired { get; private init; }
    /// <summary>
    /// Determines what input component is used to provide a default value
    /// </summary>
    public DefaultValueType DefaultValueType { get; private init; }
    /// <summary>
    /// The initial default value
    /// </summary>
    public object? InitialDefaultValue { get; set; }
    /// <summary>
    /// Allows you to provide a custom default value component
    /// </summary>
    public RenderFragment<DefaultValueRenderFragmentArgs>? DefaultValueRenderFragment { get; private init; }
    /// <summary>
    /// A default value will NOT be allowed. A csv header can be mapped but is not required for the matcher to be considered valid. 
    /// </summary>
    public static readonly ExpectedHeaderConfig Default = new ();
    /// <summary>
    ///  A default value will NOT be allowed. A csv header must be mapped or the matcher will be considered invalid.
    /// </summary>
    public static readonly ExpectedHeaderConfig Required = new (required: true);
    /// <summary>
    /// A default value with a text input will be allowed. A csv header can also be mapped but neither are required for the matcher to be considered valid. 
    /// </summary>
    public static readonly ExpectedHeaderConfig TextDefaultValue = new (defaultValueType: DefaultValueType.Text);
    /// <summary>
    /// A default value with a text input will be allowed. A csv header can also be mapped, one of either are required for the matcher to be considered valid. 
    /// </summary>
    public static readonly ExpectedHeaderConfig RequiredTextDefaultValue = new (required: true, defaultValueType: DefaultValueType.None);

    public ExpectedHeaderConfig Clone()
    {
        return (ExpectedHeaderConfig) MemberwiseClone();
    }

    public bool Equals(ExpectedHeaderConfig? other)
    {
        if (other == null) return false;
        return AutoMatching == other.AutoMatching && IsRequired == other.IsRequired &&
               DefaultValueType == other.DefaultValueType && InitialDefaultValue == other.InitialDefaultValue &&
               DefaultValueRenderFragment == other.DefaultValueRenderFragment;
    }
}

public class ExpectedHeaderConfigurator
{
    /// <summary>
    /// Sets the auto matching level for this specific property
    /// </summary>
    public AutoMatching? AutoMatching { get; private init; }
    /// <summary>
    /// If required, a default value must be provided or a header must be matched to the expected header with this config for the matcher to be considered valid.
    /// </summary>
    public bool IsRequired { get; set; }
    /// <summary>
    /// Determines what input component is used to provide a default value
    /// </summary>
    public DefaultValueType DefaultValueType { get; set; }
    /// <summary>
    /// Allows you to provide a custom default value component
    /// </summary>
    public RenderFragment<DefaultValueRenderFragmentArgs>? DefaultValueRenderFragment { get; set; }
    /// <summary>
    /// The initial default value
    /// </summary>
    public object? InitialDefaultValue { get; set; }
}

public class DefaultValueRenderFragmentArgs(bool frozen, Action<object?> defaultValueChanged)
{
    /// <summary>
    /// When true. This input should not be editable.
    /// </summary>
    public bool Frozen { get; init; } = frozen;
    /// <summary>
    /// Invoke this action when the default value changes.
    /// </summary>
    public Action<object?> DefaultValueChanged { get; init; } = defaultValueChanged;
};