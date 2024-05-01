using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;

namespace EasyCsv.Processing.Strategies;
public class TagAndReferenceMatchesStrategy : ICsvReferenceProcessor
{
    public string ColumnName { get; }
    public string ReferenceColumnName { get; }
    public int ReferenceCsvId { get; }
    public string? TagToAdd { get; }

    private readonly IEqualityComparer<string> _comparer;
    public TagAndReferenceMatchesStrategy(int referenceCsvId, string columnName, string referenceColumnName, string? tagToAdd, IEqualityComparer<string>? comparer = null)
    {
        if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentException("Column name cannot be null");
        if (string.IsNullOrWhiteSpace(referenceColumnName)) throw new ArgumentException("Reference column name cannot be null");
        if (referenceCsvId < 0) throw new ArgumentException("Reference column id must be greater than or equal to 0");
        comparer ??= EqualityComparer<string>.Default;
        ColumnName = columnName;
        ReferenceColumnName = referenceColumnName;
        _comparer = comparer;
        ReferenceCsvId = referenceCsvId;
        TagToAdd = tagToAdd;
    }

    public async Task<OperationResult> ProcessCsv(IEasyCsv csv, IEasyCsv referenceCsv)
    {
        if (!csv.ContainsColumn(ColumnName))
        {
            return new OperationResult(false, $"Csv missing column. {ColumnName}");
        }

        if (!referenceCsv.ContainsColumn(ColumnName))
        {
            return new OperationResult(false, $"Reference csv missing column. {ColumnName}");
        }
        var referenceCsvValues = referenceCsv.CsvContent.Select(x => x[ReferenceColumnName]?.ToString()).ToArray();
        var referenceMap = MapArray(referenceCsvValues, _comparer);
        int matched = 0;
        await csv.MutateAsync(x =>
        {
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            x.AddColumn(InternalColumnNames.References, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent)
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
            }
        });
        return new OperationResult(true, $"{matched} records matched and tagged.");
    }

    private static Dictionary<string, List<int>> MapArray(string?[] array, IEqualityComparer<string> comparer)
    {
        var map = new Dictionary<string, List<int>>(comparer);
        for (int i = 0; i < array.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(array[i])) continue;
            if (map.TryGetValue(array[i]!, out var list))
            {
                list.Add(i);
                continue;
            }
            map[array[i]!] = [i];
        }
        return map;
    }
}