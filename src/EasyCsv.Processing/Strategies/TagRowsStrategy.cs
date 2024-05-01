using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class TagRowsStrategy : ICsvProcessor
{
    private readonly Action<CsvRow, IList<string>> _addTagsFunc;
    public TagRowsStrategy(Action<CsvRow, IList<string>> addTagsFunc)
    {
        _addTagsFunc = addTagsFunc;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIds)
    {
        await csv.MutateAsync(x =>
        {
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIds))
            {
                var existingTags = row.Tags()?.ToList() ?? [];
                _addTagsFunc(row, existingTags);
                row[InternalColumnNames.Tags] = string.Join(",", existingTags.Distinct());
            }
        });
        return new OperationResult(true);
    }
}