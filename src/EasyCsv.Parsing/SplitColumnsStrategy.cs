using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;

namespace EasyCsv.Parsing;
public class SplitColumnStrategy : ICsvProcessor
{
    public string DisplayName => "Split Column";
    public string Description => throw new NotImplementedException();
    public List<Dictionary<string, string>> ExampleInputRows => throw new NotImplementedException();
    public List<Dictionary<string, string>> ExampleOutputRows => throw new NotImplementedException();
    private readonly string[] _newColumnNames;
    private readonly string _columnToSplit;
    private readonly string _delimiter;
    private readonly StringSplitOptions _stringSplitOptions;
    private readonly bool _removeSplitColumn;

    public SplitColumnStrategy(string columnToSplit, string[] newColumnNames, string delimiter, bool removeSplitColumn, StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries)
    {
        if (newColumnNames.Length == 0) throw new ArgumentException("New column names must be specified.");
        if (string.IsNullOrWhiteSpace(delimiter)) throw new ArgumentException("Delimiter name must be specified");
        _newColumnNames = newColumnNames;
        _delimiter = delimiter;
        _stringSplitOptions = stringSplitOptions;
        _columnToSplit = columnToSplit;
        _removeSplitColumn = removeSplitColumn;
    }

    public async Task<OperationResult> ProcessCsv(IEasyCsv csv)
    {
        if (csv.ContainsColumn(_columnToSplit))
        {
            int rowsSplit = 0;
            await csv.MutateAsync(x =>
            {
                x.AddColumns(_newColumnNames.ToDictionary(y => y, value => (object?) null), upsert: null);
                foreach (var row in x.CsvContent)
                {
                    string[]? split = row[_columnToSplit]?.ToString().Split([_delimiter], _newColumnNames.Length, _stringSplitOptions);
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
        else
        {
            return new OperationResult(false, $"Csv didn't contain column to split: '{_columnToSplit}'");
        }
    }
}