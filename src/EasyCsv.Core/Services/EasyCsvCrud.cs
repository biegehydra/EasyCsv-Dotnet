using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        public IEasyCsv InsertRow(List<object?> rowValues, int index = -1)
        {
            var headers = ColumnNames();
            if (CsvContent == null) return this;
            if (headers?.Length == null || headers.Length == 0 || headers.Length != rowValues.Count) return this;
            if (index == -1 || index >= CsvContent.Count)
            {
                var newRow = RowFromHeadersAndValues(headers, rowValues);
                CsvContent.Add(newRow);
            }
            else
            {
                var newRow = RowFromHeadersAndValues(headers, rowValues);
                CsvContent.Insert(index, newRow);
            }

            return this;
        }

        private static CsvRow RowFromHeadersAndValues(IEnumerable<string> headers, List<object?> values)
        {
            var row = new CsvRow(headers, values);
            return row;
        }

        public IEasyCsv UpsertRow(CsvRow row, int index = -1)
        {
            if (CsvContent == null) return this;

            if (index >= 0 && index < CsvContent.Count)
            {
                CsvContent[index] = row;
            }
            else
            {
                int existingIndex = CsvContent.FindIndex(r => RowsEqual(r, row));
                if (existingIndex >= 0)
                {
                    CsvContent[existingIndex] = row;
                }
                else
                {
                    CsvContent.Add(row);
                }
            }

            return this;
        }

        public IEasyCsv AddRow(CsvRow row) => InsertRow(row);
        public IEasyCsv InsertRow(CsvRow row, int index = -1)
        {
            var headers = ColumnNames();
            if (CsvContent == null) return this;
            if (headers?.Length == null || headers.Length == 0 || headers.Length != row.Count) return this;
            if (index == -1 || index >= CsvContent.Count)
            {
                CsvContent.Add(row);
            }
            else
            {
                CsvContent.Insert(index, row);
            }

            return this;
        }

        public IEasyCsv UpsertRows(IEnumerable<CsvRow> rows)
        {
            foreach (var row in rows)
            {
                UpsertRow(row);
            }

            return this;
        }

        public CsvRow? GetRow(int index)
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return null;
            }

            return CsvContent.ElementAt(index);
        }

        public T? GetRow<T>(int index) where T : class
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return null;
            }
            var row = GetRow(index);
            if (row == null) return null;
            var content = new List<CsvRow>() { row };
            var easyCsv = new EasyCsv(content, Config);
            if(easyCsv.ContentBytes == null) return null;
            using var reader = new StreamReader(new MemoryStream(easyCsv.ContentBytes), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, Config.CsvHelperConfig);
            return csvReader.GetRecord<T>();
        }

        public IEasyCsv UpdateRow(int index, CsvRow newRow)
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return this;
            }

            CsvContent[index] = newRow;
            return this;
        }

        public IEasyCsv DeleteRow(int index)
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return this;
            }

            CsvContent.RemoveAt(index);
            return this;
        }

        public IEasyCsv DeleteRows(IEnumerable<int> indexes)
        {
            if (CsvContent == null)
            {
                return this;
            }

            var indexesArr = indexes.ToArray();
            foreach (var index in indexesArr)
            {
                if (!Utils.IsValidIndex(index, CsvContent.Count))
                {
                    return this;
                }
            }

            foreach (var index in indexesArr.Distinct().OrderByDescending(x => x))
            {
                CsvContent.RemoveAt(index);
            }

            return this;
        }
    }
}