using System;
using System.Collections.Generic;
using System.Linq;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class FindDedupesExactMatchColumnStrategy : IFindDedupesOperation
{
    public string ColumnName { get; }
    public IEqualityComparer<string> Comparer { get; }

    public FindDedupesExactMatchColumnStrategy(string columnName, IEqualityComparer<string>? comparer = null)
    {
        if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentException("Column name can't be empty", nameof(columnName));
        ColumnName = columnName;
        comparer ??= EqualityComparer<string>.Default;
        Comparer = comparer;

    }
    public async IAsyncEnumerable<DuplicateGrouping> YieldReturnDupes(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null, params (IEasyCsv Csv, int ReferenceCsvId)[] referenceCsvs)
    {
        var csvValues = csv.CsvContent.Select(x => x[ColumnName]?.ToString()).ToArray();
        var csvMap = StrategyHelpers.MapArray(csvValues, Comparer);
        int matched = 0;
        HashSet<int> processed = new HashSet<int>();
        int i = -1;
        foreach (var (row, index) in csv.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIndexes))
        {
            if (processed.Contains(index)) continue;
            i++;
            var value = row[ColumnName]?.ToString();
            List<int>? rowIds = null;
            if (!string.IsNullOrWhiteSpace(value) && csvMap.TryGetValue(value!, out rowIds) &&
                rowIds?.Count > 1)
            {
                var duplicateGrouping = new DuplicateGrouping([(null, rowIds.Select(x => (x, csv.CsvContent[x])))]);
                yield return duplicateGrouping;
            }

            if (rowIds?.Count > 0)
            {
                foreach (var rowId in rowIds)
                {
                    processed.Add(rowId);
                }
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Unexpected behaviour");
            }
        }
    }
}