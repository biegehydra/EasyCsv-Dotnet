using System.Collections.Generic;

namespace EasyCsv.Core
{
    public interface IInternalCrud
    {
        internal IEasyCsv AddRecord(List<string> rowValues, int index = -1);
        internal IEasyCsv InsertRecord(List<string> rowValues, int index = -1);
        internal IEasyCsv UpsertRecord(CsvRow row, int index = -1);
        internal IEasyCsv UpsertRecords(IEnumerable<CsvRow> rows);
        internal IEasyCsv UpdateRecord(int index, CsvRow newRow);
        internal IEasyCsv DeleteRecord(int index);
    }
}