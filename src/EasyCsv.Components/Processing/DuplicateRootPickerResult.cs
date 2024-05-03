using EasyCsv.Core;

namespace EasyCsv.Components.Processing;
public class DuplicateRootPickerResult(CsvRow rowToKeep, int? referenceCsvId)
{
    public CsvRow RowToKeep { get; } = rowToKeep;
    public int? ReferenceCsvId { get; } = referenceCsvId;
}

public class MultiDuplicateRootPickerResult(HashSet<(CsvRow RowToKeep, int? ReferenceCsvId)> rowsToKeep)
{
    public HashSet<(CsvRow RowToKeep, int? ReferenceCsvId)> RowsToKeep { get; } = rowsToKeep;
}