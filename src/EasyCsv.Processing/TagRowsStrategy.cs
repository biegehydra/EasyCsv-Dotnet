using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;

namespace EasyCsv.Processing;
public class TagRowsStrategy : ICsvProcessor
{
    private readonly Action<CsvRow, IList<string>> _addTagsFunc;
    public TagRowsStrategy(Action<CsvRow, IList<string>> addTagsFunc)
    {
        _addTagsFunc = addTagsFunc;
    }

    public async Task<OperationResult> ProcessCsv(IEasyCsv csv)
    {
        await csv.MutateAsync(x =>
        {
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent)
            {
                var existingTags = row[InternalColumnNames.Tags]?.ToString()?.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();
                existingTags ??= new List<string>();
                _addTagsFunc(row, existingTags);
                row[InternalColumnNames.Tags] = string.Join(",", existingTags.Distinct());
            }
        });
        return new OperationResult(true);
    }
}