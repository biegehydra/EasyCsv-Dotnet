using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

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
    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        if (!csv.ContainsColumn(_columnName))
        {
            return new OperationResult(false, $"Column to divide and replicate on did not exist. Column Name: {_columnName}");
        }

        int deletedCount = 0;
        int replicatedCount = 0;
        await csv.MutateAsync(x =>
        {
            var rowsWithIndexes = x.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIndexes).ToArray();
            foreach (var (row, origIndex) in rowsWithIndexes.OrderByDescending(y => y.Item2))
            {
                var divided = _divideFunc(row[_columnName]);
                if (divided?.Length > 1)
                {
                    x.CsvContent.RemoveAt(origIndex);
                    deletedCount++;
                    foreach (var item in divided)
                    {
                        var clone = row.Clone();
                        clone[_columnName] = item;
                        x.CsvContent.Insert(origIndex, clone);
                        replicatedCount++;
                    }
                }
            }
        });
        return new OperationResult(true, $"Replicated {deletedCount} rows into {replicatedCount}");
    }
}