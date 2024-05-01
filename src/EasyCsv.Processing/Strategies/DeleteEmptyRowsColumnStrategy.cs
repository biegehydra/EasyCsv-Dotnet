using System;
using System.Threading.Tasks;
using EasyCsv.Core;

namespace EasyCsv.Processing.Strategies;
public class DeleteEmptyRowsColumnStrategy : ICsvColumnDeleteEvaluator
{
    public string ColumnName { get; }

    private readonly Func<object?, object?> _transformValue;
    public DeleteEmptyRowsColumnStrategy(string columnName, Func<object?, object?> transformValue)
    {
        ColumnName = columnName;
        _transformValue = transformValue;
    }
    public Task<OperationDeleteResult> EvaluateDelete<TCell>(TCell cell) where TCell : ICell
    {
        throw new NotImplementedException();
    }
}