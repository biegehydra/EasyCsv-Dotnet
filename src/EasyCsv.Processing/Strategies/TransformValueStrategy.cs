using System;
using System.Threading.Tasks;
namespace EasyCsv.Processing.Strategies;
public class TransformValueStrategy : ICsvColumnProcessor
{
    public string ColumnName { get; }
    private readonly Func<object?, object?> _transformValue;
    public TransformValueStrategy(string columnName, Func<object?, object?> transformValue)
    {
        ColumnName = columnName;
        _transformValue = transformValue;
    }

    public ValueTask<OperationResult> ProcessCell<TCell>(TCell cell) where TCell : ICell
    {
        cell.Value = _transformValue(cell.Value);
        return new ValueTask<OperationResult>(new OperationResult(true));
    }
}