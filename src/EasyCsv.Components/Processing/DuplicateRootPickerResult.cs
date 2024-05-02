using EasyCsv.Core;

namespace EasyCsv.Components.Processing;
public class DuplicateRootPickerResult(CsvRow selectedRow, int? referenceCsvId)
{
    public CsvRow SelectedRow { get; } = selectedRow;
    public int? ReferenceCsvId { get; } = referenceCsvId;
}