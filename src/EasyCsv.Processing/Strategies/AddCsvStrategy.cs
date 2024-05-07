using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class AddFullCsvStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => false;
    private readonly IEasyCsv _csvToAdd;
    public AddFullCsvStrategy(IEasyCsv csvToAdd)
    {
        _csvToAdd = csvToAdd ?? throw new ArgumentException("CsvToAdd can't be null.", nameof(csvToAdd));
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        var columnNames = csv.ColumnNames()?.ToHashSet();
        if (columnNames == null) return new OperationResult(false, "WorkingCsv has no column names");
        var toAddColumnNames = _csvToAdd.ColumnNames()?.ToHashSet();
        if (toAddColumnNames == null) return new OperationResult(false, "CsvToAdd has no column names");
        await csv.MutateAsync(x =>
        {
            foreach (var additionalRow in _csvToAdd.CsvContent)
            {
                var newRow = new Dictionary<string, object?>(columnNames.Count);
                foreach (var baseColumnName in columnNames)
                {
                    // Column name mapped from base to additional
                    if (toAddColumnNames.Contains(baseColumnName))
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
        return new OperationResult(true, $"Added {_csvToAdd.CsvContent.Count} rows to WorkingCsv");
    }
}