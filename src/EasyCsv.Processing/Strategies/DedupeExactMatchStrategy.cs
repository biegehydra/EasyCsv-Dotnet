using System;
using System.Collections.Generic;
using System.Linq;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class FindDedupesExactMatchColumnStrategy : IFindDedupesOperation
{
    public string TitleText { get; }
    public string ColumnName { get; }
    public IEqualityComparer<string> Comparer { get; }
    public bool MultiSelect { get; }
    public bool MustSelectRow { get; }

    public Func<string?, string?>? DuplicateValuePresenter { get; }

    public FindDedupesExactMatchColumnStrategy(string columnName, bool mustSelectRow, bool multiSelect = true, Func<string?, string?>? duplicateValuePresenter = null, IEqualityComparer<string>? comparer = null)
    {
        if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentException("Column name can't be empty", nameof(columnName));
        TitleText = $"Duplicate Resolver ({columnName})";
        ColumnName = columnName;
        MustSelectRow = mustSelectRow;
        DuplicateValuePresenter = duplicateValuePresenter;
        MultiSelect = multiSelect;
        comparer ??= EqualityComparer<string>.Default;
        Comparer = comparer;

    }


    public async IAsyncEnumerable<DuplicateGrouping> YieldReturnDupes(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null, params (IEasyCsv Csv, int ReferenceCsvId)[] referenceCsvs)
    {
        var csvValues = csv.CsvContent.Select(x => x[ColumnName]?.ToString()).ToArray();
        var csvMap = StrategyHelpers.MapArray(csvValues, Comparer);
        HashSet<int> processed = new HashSet<int>();
        foreach (var (row, index) in csv.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIndexes))
        {
            if (processed.Contains(index)) continue;
            var value = row[ColumnName]?.ToString();
            List<int>? rowIds = null;
            if (!string.IsNullOrWhiteSpace(value) && csvMap.TryGetValue(value!, out rowIds) &&
                rowIds?.Count > 1)
            {
                var duplicateGrouping = new DuplicateGrouping(value, [(null, rowIds.Select(x => (x, csv.CsvContent[x])))]);
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