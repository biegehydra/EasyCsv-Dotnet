using System;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Enums;

namespace EasyCsv.Processing.Strategies;

public class SplitColumnStrategy : ICsvProcessor
{
    private readonly string[] _newColumnNames;
    private readonly string _columnToSplit;
    private readonly bool _removeSplitColumn;
    private readonly Func<object?, object?[]?> _splitFunc;

    public SplitColumnStrategy(string columnToSplit, string[] newColumnNames, bool removeSplitColumn, Func<object?, object?[]?> splitFunc)
    {
        if (newColumnNames.Length == 0) throw new ArgumentException("New column names must be specified.");
        _newColumnNames = newColumnNames;
        _columnToSplit = columnToSplit;
        _removeSplitColumn = removeSplitColumn;
        _splitFunc = splitFunc ?? throw new ArgumentException("SplitFunc cannot be null");
    }

    public async Task<OperationResult> ProcessCsv(IEasyCsv csv)
    {
        if (csv.ContainsColumn(_columnToSplit))
        {
            int rowsSplit = 0;
            await csv.MutateAsync(x =>
            {
                x.AddColumns(_newColumnNames.ToDictionary(y => y, value => (object?)null), existingColumnHandling: ExistingColumnHandling.Keep);
                foreach (var row in x.CsvContent)
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
                }
                if (_removeSplitColumn)
                {
                    x.RemoveColumn(_columnToSplit);
                }
            });
            return new OperationResult(true, $"Split succeeded on {rowsSplit} rows");
        }
        return new OperationResult(false, $"Csv didn't contain column to split: '{_columnToSplit}'");
    }
}