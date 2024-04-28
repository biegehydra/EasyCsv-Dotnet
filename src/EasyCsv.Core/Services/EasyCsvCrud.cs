using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        public IEasyCsv InsertRecord(List<object?> rowValues, int index = -1)
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

        public IEasyCsv UpsertRecord(CsvRow row, int index = -1)
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

        public IEasyCsv UpsertRecords(IEnumerable<CsvRow> rows)
        {
            foreach (var row in rows)
            {
                UpsertRecord(row);
            }

            return this;
        }

        public CsvRow? GetRecord(int index)
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return null;
            }

            return CsvContent.ElementAt(index);
        }

        public T? GetRecord<T>(int index) where T : class
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return null;
            }
            var row = GetRecord(index);
            if (row == null) return null;
            var content = new List<CsvRow>() { row };
            var easyCsv = new EasyCsv(content, Config);
            if(easyCsv.ContentBytes == null) return null;
            using var reader = new StreamReader(new MemoryStream(easyCsv.ContentBytes), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, Config.CsvHelperConfig);
            return csvReader.GetRecord<T>();
        }

        public IEasyCsv UpdateRecord(int index, CsvRow newRow)
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return this;
            }

            CsvContent[index] = newRow;
            return this;
        }

        public IEasyCsv DeleteRecord(int index)
        {
            if (CsvContent == null || index < 0 || index >= CsvContent.Count)
            {
                return this;
            }

            CsvContent.RemoveAt(index);
            return this;
        }
    }
}