namespace EasyCsv.Components.Processing;
public enum StringPredicates
{
    Equals,
    Contains, // User can enter multiple values comma separated in a mud text field component
    StartsWith, // User can enter multiple values comma separated in a mud text field component
    EndsWith, // User can enter multiple values comma separated in a mud text field component
    IsEmptyOrWhitespace,
    LengthEquals,
    LengthGreaterThan,
    LengthLessThan,
    MatchesRegex,
    ContainsAll, 
    IsNumeric,
    IsAlphabetical,
    IsAlphanumeric
}
