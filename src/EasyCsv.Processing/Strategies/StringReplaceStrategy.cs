using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyCsv.Processing.Strategies;
public class StringReplaceStrategy : ICsvColumnProcessor
{
    public readonly string ValueToReplace;
    public readonly string NewValue;
    public readonly StringComparison StringComparison;
    public bool OperatesOnlyOnFilteredRows => true;
    public string ColumnName { get; }
    public int ReplacedCount { get; private set; }
    public StringReplaceStrategy(string columnName, string valueToReplace, string newValue, StringComparison stringComparison)
    {
        ValueToReplace = valueToReplace;
        NewValue = newValue;
        StringComparison = stringComparison;
        ColumnName = columnName;
    }
    public ValueTask<OperationResult> ProcessCell<TCell>(ref TCell cell) where TCell : ICell
    {
        string? beforeValue = cell.Value?.ToString();
#if NETSTANDARD2_0
        cell.Value = cell.Value?.ToString()?.Replace(ValueToReplace, NewValue);
#else
        cell.Value = cell.Value?.ToString()?.Replace(ValueToReplace, NewValue, StringComparison);
#endif
        if (string.Equals(beforeValue, cell.Value?.ToString()))
        {
            ReplacedCount++;
        }
        return new ValueTask<OperationResult>(new OperationResult(true));
    }
}
