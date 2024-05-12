using System.Threading.Tasks;
using System;
using EasyCsv.Core;

namespace EasyCsv.Processing.Strategies;
public class ShouldDeleteRowStrategy : ICsvRowDeleteEvaluator
{
    public bool OperatesOnlyOnFilteredRows => true;
    public string ColumnName { get; }
    private readonly Func<CsvRow, bool> _shouldDeleteFunc;
    public ShouldDeleteRowStrategy(string columnName, Func<CsvRow, bool> shouldDeleteFunc)
    {
        ColumnName = columnName;
        _shouldDeleteFunc = shouldDeleteFunc;
    }

    public ValueTask<OperationDeleteResult> EvaluateDelete(CsvRow row)
    {
        if (_shouldDeleteFunc(row))
        {
            return new ValueTask<OperationDeleteResult>(new OperationDeleteResult(true, false));
        }
        return new ValueTask<OperationDeleteResult>(new OperationDeleteResult(true, false));
    }
}