using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;

public class SplitColumnStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    private readonly string[] _newColumnNames;
    private readonly string _columnToSplit;
    private readonly bool _removeSplitColumn;
    private readonly Func<object?, object?[]?> _splitFunc;
    private readonly Func<double, Task>? _onProgress;

    public SplitColumnStrategy(string columnToSplit, string[] newColumnNames, bool removeSplitColumn, Func<object?, object?[]?> splitFunc, Func<double, Task>? onProgress = null)
    {
        if (newColumnNames.Length == 0) throw new ArgumentException("New column names must be specified.");
        _newColumnNames = newColumnNames;
        _columnToSplit = columnToSplit;
        _removeSplitColumn = removeSplitColumn;
        _splitFunc = splitFunc ?? throw new ArgumentException("SplitFunc cannot be null");
        _onProgress = onProgress;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        if (csv.ContainsColumn(_columnToSplit))
        {
            int rowsSplit = 0;
            await csv.MutateAsync(async x =>
            {
                int total = filteredRowIndexes?.Count ?? csv.CsvContent.Count;
                int processed = 0;
                x.AddColumns(_newColumnNames.ToDictionary(y => y, value => (object?)null), existingColumnHandling: ExistingColumnHandling.Keep);
                foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIndexes))
                {
                    object?[]? split = _splitFunc(row[_columnToSplit]);
                    if (split?.Length > 0)
                    {
                        rowsSplit++;
                        for (int i = 0; i < split.Length; i++)
                        {
                            row[_newColumnNames[i]] = split[i];
                        }
                    }
                    if (_onProgress != null)
                    {
                        processed++;
                        if (_removeSplitColumn)
                        {
                            await _onProgress(((double)processed / total * 0.9) + 0.1);
                        }
                        else
                        {
                            await _onProgress((double)processed / total);
                        }
                    }
                }
                if (_removeSplitColumn)
                {
                    x.RemoveColumn(_columnToSplit);
                    if (_onProgress != null)
                    {
                        await _onProgress(1);
                    }
                }
            }, saveChanges: false);
            return new OperationResult(true, $"Split succeeded on {rowsSplit} rows");
        }
        return new OperationResult(false, $"Csv didn't contain column to split: '{_columnToSplit}'");
    }
}