using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;

namespace EasyCsv.Processing.Strategies;
public class TagRowsAsyncStrategy : ICsvProcessor
{
    public string ColumnName { get; }
    private readonly Func<CsvRow, IList<string>, Task> _addTagsFunc;
    public TagRowsAsyncStrategy(string columnName, Func<CsvRow, IList<string>, Task> addTagsFunc)
    {
        ColumnName = columnName;

    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv)
    {
        await csv.MutateAsync(async x =>
        {
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent)
            {
                var existingTags = row[InternalColumnNames.Tags]?.ToString()?.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();
                existingTags ??= new List<string>();
                await _addTagsFunc(row, existingTags);
                row[InternalColumnNames.Tags] = string.Join(",", existingTags.Distinct());
            }
        });
        return new OperationResult(true);
    }
}