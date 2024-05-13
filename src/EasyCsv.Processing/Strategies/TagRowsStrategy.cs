using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class TagRowsStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    private readonly Action<CsvRow, IList<string>> _addTagsFunc;
    private readonly Func<double, Task>? _onProgress;

    public TagRowsStrategy(Action<CsvRow, IList<string>> addTagsFunc, Func<double, Task>? onProgress = null)
    {
        _addTagsFunc = addTagsFunc;
        _onProgress = onProgress;
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
                _addTagsFunc(row, existingTags);
                row[InternalColumnNames.Tags] = string.Join(",", existingTags.Distinct());
                if (_onProgress != null)
                {
                    processed++;
                    await _onProgress((double)processed / total);
                }
            }
        }, saveChanges: false);
        return new OperationResult(true);
    }
}