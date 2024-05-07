using EasyCsv.Core;

namespace EasyCsv.Components.Processing;
public class DuplicateRootPickerResult(CsvRow? rowToKeep, int? rowIndex)
{
    public CsvRow? RowToKeep { get; } = rowToKeep;
    public int? RowIndex { get; } = rowIndex;
}

public class MultiDuplicateRootPickerResult(HashSet<(int RowIndex, CsvRow RowToKeep)> rowsToKeep)
{
    public HashSet<(int RowIndex, CsvRow RowToKeep)> RowsToKeep { get; } = rowsToKeep;
}