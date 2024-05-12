using System.Collections.Generic;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;
using Enumerable = System.Linq.Enumerable;

namespace EasyCsv.Processing.Strategies;
internal static class AddRowsHelper
{
    public static (OperationResult, List<CsvRow>) CreateSameStructureRows(ICollection<CsvRow> rowsToAdd, IEasyCsv csv)
    {
        var columnNames = csv.ColumnNames()?.ToHashSet();
        if (columnNames == null) return (new OperationResult(false, "WorkingCsv has no column names"), new ());
        var toAddColumnNames = Enumerable.FirstOrDefault(rowsToAdd)?.Keys?.ToHashSet();
        if (toAddColumnNames == null) return (new OperationResult(false, "CsvToAdd has no column names"), new ());
        List<CsvRow> correctStructureRows = new();
        foreach (var additionalRow in rowsToAdd)
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
            correctStructureRows.Add(new CsvRow(newRow, true));
        }
        return (new OperationResult(true, $"Added {rowsToAdd.Count} rows to WorkingCsv"), correctStructureRows);
    }
}