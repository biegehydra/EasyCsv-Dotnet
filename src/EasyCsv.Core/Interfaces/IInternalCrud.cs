using System.Collections.Generic;

namespace EasyCsv.Core
{
    public interface IInternalCrud
    {
        internal IEasyCsv InsertRow(List<object?> rowValues, int index = -1);
        internal IEasyCsv UpsertRow(CsvRow row, int index = -1);
        internal IEasyCsv UpsertRows(IEnumerable<CsvRow> rows);
        internal IEasyCsv UpdateRow(int index, CsvRow newRow);
        internal IEasyCsv DeleteRow(int index);
        internal IEasyCsv DeleteRow(CsvRow row);
        internal IEasyCsv DeleteRows(IEnumerable<int> index);
    }
}