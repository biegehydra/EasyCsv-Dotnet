using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;

namespace EasyCsv.Processing.Strategies;
public class DivideAndReplicateStrategy : ICsvProcessor
{
    private readonly string _columnName;
    private readonly Func<object?, object?[]?> _divideFunc;
    public DivideAndReplicateStrategy(string columnName, Func<object?, object?[]?> divideFunc)
    {
        if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentException("Column name cannot be null or whitespace.", nameof(columnName));
        _columnName = columnName;
        _divideFunc = divideFunc ?? throw new ArgumentException("DivideFunc cannot be null.", nameof(divideFunc));
    }
    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv)
    {
        if (!csv.ContainsColumn(_columnName))
        {
            return new OperationResult(false, $"Column to divide and replicate on did not exist. Column Name: {_columnName}");
        }
        List<int> rowsToDelete = new();
        List<CsvRow> rowsToAdd = new();
        await csv.MutateAsync(x =>
        {
            int i = 0;
            foreach (var row in x.CsvContent)
            {
                var divided = _divideFunc(row[_columnName]);
                if (divided?.Length > 1)
                {
                    rowsToDelete.Add(i);
                    foreach (var item in divided)
                    {
                        var clone = row.Clone();
                        clone[_columnName] = item;
                        rowsToAdd.Add(clone);
                    }
                }

                i++;
            }

            foreach (var rowToDelete in rowsToDelete.OrderByDescending(y => y))
            {
                x.CsvContent.RemoveAt(rowToDelete);
            }

            foreach (var rowToAdd in rowsToAdd)
            {
                x.CsvContent.Add(rowToAdd);
            }
        });
        return new OperationResult(true, $"Replicated {rowsToDelete.Count} rows int {rowsToAdd.Count}");
    }
}