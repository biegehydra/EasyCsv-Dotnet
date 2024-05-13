using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class TagRowsAsyncStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    public string ColumnName { get; }
    private readonly Func<CsvRow, IList<string>, Task> _addTagsFunc;
    private readonly Func<double, Task>? _onProgress;

    public TagRowsAsyncStrategy(string columnName, Func<CsvRow, IList<string>, Task> addTagsFunc, Func<double, Task>? onProgress = null)
    {
        _addTagsFunc = addTagsFunc;
        _onProgress = onProgress;
        ColumnName = columnName;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes)
    {
        int total = filteredRowIndexes?.Count ?? csv.CsvContent.Count;
        int processed = 0;
        await csv.MutateAsync(async x =>
        {
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIndexes))
            {
                var existingTags = row.ProcessingTags()?.ToList() ?? [];
                await _addTagsFunc(row, existingTags);
                row[InternalColumnNames.Tags] = string.Join(",", existingTags.Distinct());
                if (_onProgress != null)
                {
                    processed++;
                    await _onProgress((double) processed / total);
                }
            }
        }, saveChanges: false);
        return new OperationResult(true);
    }
}