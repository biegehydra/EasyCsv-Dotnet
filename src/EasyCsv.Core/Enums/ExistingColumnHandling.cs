using System.ComponentModel;

namespace EasyCsv.Core.Enums;
public enum ExistingColumnHandling
{
    [Description("If the specified column already exists, the value will be overriden in every row.")]
    Override,
    [Description("If the specified column already exists, the value in a row will be ovverriden if it is null or whitespace.")]
    ReplaceNullOrWhiteSpace,
    [Description("If the specified column already exists, the value will not be changed in any row.")]
    Keep,
    [Description("An exception will be thrown if the specified column already exists")]
    ThrowException
}