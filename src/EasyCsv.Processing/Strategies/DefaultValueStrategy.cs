using System;
using System.Threading.Tasks;
namespace EasyCsv.Processing.Strategies;
public class DefaultValueStrategy : ICsvColumnProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    public string ColumnName { get; }
    private readonly object? _defaultValue;
    private readonly Func<object?, bool> _shouldOverride;
    public DefaultValueStrategy(string columnName, object? defaultValue, Func<object?, bool>? shouldOverride = null)
    {
        ColumnName = columnName;
        _defaultValue = defaultValue;
        _shouldOverride = shouldOverride ?? (x => true);
    }

    public ValueTask<OperationResult> ProcessCell<TCell>(ref TCell cell) where TCell : ICell
    {
        if (_shouldOverride(cell.Value))
        {
            cell.Value = _defaultValue;
        }
        return new ValueTask<OperationResult>(new OperationResult(true));
    }
}