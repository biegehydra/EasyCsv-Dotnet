using System.ComponentModel;

namespace EasyCsv.Components;
/// <summary>
/// Controls how csv headers are automatically mapped to expected headers.
/// </summary>
public enum AutoMatching
{
    [Description("Matches only when the strings are an exact match (Case Insensitive): Ex: 'First Name' and 'first name'")]
    Exact,
    [Description("Matches when the strings are a close match: Ex: 'First Name' and 'first nme'")]
    Strict,
    [Description("Matches even partial strings: Ex: 'First Name' and 'first'")]
    Lenient
}

/// <summary>
/// Represents the current state of an expected header.
/// </summary>
public enum MatchState
{
    [Description("The expected header has been matched to a csv header.")]
    Mapped,
    [Description("The expected header has been provided with a default value..")]
    ValueProvided,
    [Description("The expected header is required and is not mapped and does not have a value provided.")]
    RequiredAndMissing,
    [Description("The expected header is NOT required and is not mapped and does not have a value provided.")]
    Missing
}

public enum DefaultValueType
{
    [Description("Users will not be able to provide a default value.")]
    None,
    [Description("The input will be MudTextField.")]
    Text,
    [Description("The input will be MudDatePicker.")]
    DateTime,
    [Description("The input will be a MudNumericalField.")]
    Numerical,
    [Description("The input will be a MudCheckBox.")]
    CheckBox,
    [Description("The input will be a MudCheckBox with tri state enabled.")]
    TriStateCheckBox
}

public enum ColumnValueType
{
    [Description("The input will be MudTextField.")]
    Text,
    SelectFromAll,
    [Description("The input will be MudDatePicker.")]
    DateTime,
    [Description("The input will be a MudNumericalField.")]
    Integer,
    Decimal
}

public enum ResolveDuplicatesAutoSelect
{
    [Description("Do not auto select any row.")]
    None,
    [Description("Auto select the first row.")]
    FirstRow
}