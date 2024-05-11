using System;
using System.Threading.Tasks;

namespace EasyCsv.Processing.Strategies;

public class ShouldDeleteColumnStrategy : ICsvColumnDeleteEvaluator
{
    public bool OperatesOnlyOnFilteredRows => true;
    public string ColumnName { get; }
    private readonly Func<IReadonlyCell, bool> _shouldDeleteFunc;
    public ShouldDeleteColumnStrategy(string columnName, Func<IReadonlyCell, bool> shouldDeleteFunc)
    {
        ColumnName = columnName;
        _shouldDeleteFunc = shouldDeleteFunc;
    }

    public ValueTask<OperationDeleteResult> EvaluateDelete<TCell>(TCell cell) where TCell : IReadonlyCell
    {
        if (_shouldDeleteFunc(cell))
        {
            return new ValueTask<OperationDeleteResult>(new OperationDeleteResult(true, true));
        }
        return new ValueTask<OperationDeleteResult>(new OperationDeleteResult(true, false));
    }
}