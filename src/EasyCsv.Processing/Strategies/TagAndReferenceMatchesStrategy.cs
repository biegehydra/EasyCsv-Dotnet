﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class TagAndReferenceMatchesStrategy : ICsvReferenceProcessor
{
    private readonly Func<double, Task>? _onProgress;
    public bool OperatesOnlyOnFilteredRows => true;
    public string ColumnName { get; }
    public string ReferenceColumnName { get; }
    public int ReferenceCsvId { get; }
    public string? TagToAdd { get; }

    public IEqualityComparer<string> Comparer { get; }
    public TagAndReferenceMatchesStrategy(int referenceCsvId, string columnName, string referenceColumnName, string? tagToAdd, IEqualityComparer<string>? comparer = null, Func<double, Task>? onProgress = null)
    {
        if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentException("Column name cannot be null");
        if (string.IsNullOrWhiteSpace(referenceColumnName)) throw new ArgumentException("Reference column name cannot be null");
        if (referenceCsvId < 0) throw new ArgumentException("Reference column id must be greater than or equal to 0");
        _onProgress = onProgress;
        comparer ??= EqualityComparer<string>.Default;
        ColumnName = columnName;
        ReferenceColumnName = referenceColumnName;
        Comparer = comparer;
        ReferenceCsvId = referenceCsvId;
        TagToAdd = tagToAdd;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, IEasyCsv referenceCsv, ICollection<int>? filteredRowIndexes)
    {
        if (!csv.ContainsColumn(ColumnName))
        {
            return new OperationResult(false, $"Csv missing column. {ColumnName}");
        }

        if (!referenceCsv.ContainsColumn(ReferenceColumnName))
        {
            return new OperationResult(false, $"Reference csv missing column. {ColumnName}");
        }
        var referenceCsvValues = referenceCsv.CsvContent.Select(x => x[ReferenceColumnName]?.ToString()).ToArray();
        var referenceMap = StrategyHelpers.MapArray(referenceCsvValues, Comparer);
        int matched = 0;
        await csv.MutateAsync(async x =>
        {
            int total = filteredRowIndexes?.Count ?? csv.CsvContent.Count;
            int processed = 0;
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            x.AddColumn(InternalColumnNames.References, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIndexes))
            {
                var value = row[ColumnName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value) && referenceMap.TryGetValue(value!, out var referenceIds) &&
                    referenceIds.Count > 0)
                {
                    matched++;
                    row.AddProcessingReferences(ReferenceCsvId, referenceIds);
                    if (!string.IsNullOrWhiteSpace(TagToAdd))
                    {
                        row.AddProcessingTags([TagToAdd]);
                    }
                }
                if (_onProgress != null)
                {
                    processed++;
                    await _onProgress((double)processed / total);
                }
            }
        }, saveChanges: false);
        return new OperationResult(true, $"{matched} records matched and tagged.");
    }
}