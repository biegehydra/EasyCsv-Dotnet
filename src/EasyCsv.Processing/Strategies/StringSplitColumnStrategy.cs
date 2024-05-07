using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCsv.Core;

namespace EasyCsv.Processing.Strategies;
public class StringSplitColumnStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    private readonly SplitColumnStrategy _splitColumnStrategy;
    public StringSplitColumnStrategy(string columnToSplit, string[] newColumnNames, string delimiter, bool removeSplitColumn, StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries)
    {
        if (newColumnNames.Length == 0) throw new ArgumentException("New column names must be specified.");
        if (string.IsNullOrWhiteSpace(delimiter)) throw new ArgumentException("Delimiter name must be specified");
        _splitColumnStrategy = new SplitColumnStrategy(columnToSplit, newColumnNames, removeSplitColumn,
            x => x?.ToString()?.Split([delimiter], stringSplitOptions)?.Cast<object?>().ToArray());
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        return await _splitColumnStrategy.ProcessCsv(csv, filteredRowIndexes);
    }
}