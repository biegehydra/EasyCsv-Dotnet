using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Parsing;
public class MergeCsvsStrategy : ICsvMerger
{
    private readonly MergeConfig _mergeConfig;
    public MergeCsvsStrategy(MergeConfig mergeConfig)
    {
        _mergeConfig = mergeConfig ?? throw new ArgumentException("Merge config cannot be null", nameof(mergeConfig));
    }
    public async Task<IEasyCsv> Merge(IEasyCsv baseCsv, IEasyCsv additionalCsv)
    {
        var columnNames = baseCsv.ColumnNames();
        if (columnNames == null) return baseCsv;
        var newColumns = _mergeConfig.NewColumns.Except(columnNames);
        await baseCsv.MutateAsync(x =>
        {
            x.AddColumns(newColumns.ToDictionary(y => y, y => (object?) null), ExistingColumnHandling.Keep);
            var allColumnNames = x.ColumnNames()!;
            foreach (var additionalRow in additionalCsv.CsvContent)
            {
                var newRow = new Dictionary<string, object?>(allColumnNames.Length);
                foreach (var baseColumnName in allColumnNames)
                {
                    // Column name mapped from base to additional
                    if (_mergeConfig.ColumnMappings.TryGetValue(baseColumnName, out var additionalColumnName))
                    {
                        newRow.Add(baseColumnName, additionalRow[additionalColumnName]);
                    }
                    else if (_mergeConfig.NewColumns.Contains(baseColumnName))
                    {
                        newRow.Add(baseColumnName, additionalRow[baseColumnName]);
                    }
                    else
                    {
                        // Column name exists in baseCsv but not additionalCsv
                        newRow.Add(baseColumnName, null);
                    }
                }

                x.CsvContent.Add(new CsvRow(newRow, true));
            }
        });
        return baseCsv;
    }
}

public class MergeConfig
{
    public MergeConfig(string[] newColumns, ColumnMapping[] columnMappings)
    {
        if (columnMappings.GroupBy(x => x.BaseColumnName).Any(x => x.Count() > 1) ||
            columnMappings.GroupBy(x => x.AdditionalColumnName).Any(x => x.Count() > 1))
        {
            throw new ArgumentException("Duplicate column error. Column mappings must be one to one.");
        }
        ColumnMappings = columnMappings.ToDictionary(x => x.BaseColumnName, x => x.AdditionalColumnName);
        NewColumns = newColumns.ToHashSet();
    }
    internal Dictionary<string, string> ColumnMappings { get; }
    internal HashSet<string> NewColumns { get; }
}

public readonly record struct ColumnMapping()
{
    public string BaseColumnName { get; }
    public string AdditionalColumnName { get; }
    public ColumnMapping(string baseColumnName, string additionalColumnName) : this()
    {
        BaseColumnName = baseColumnName;
        AdditionalColumnName = additionalColumnName;
    }
};