using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        public IEasyCsv AddRecords(List<string> rowValues, int index = -1)
        {
            var headers = GetHeaders();
            if (Content == null) return this;
            if (headers?.Count == null || headers.Count == 0 || headers.Count != rowValues.Count) return this;
            if (index == -1 || index >= Content.Count)
            {
                var newRow = RowFromHeadersAndValues(headers, rowValues);
                Content.Add(newRow);
            }
            else
            {
                var newRow = RowFromHeadersAndValues(headers, rowValues);
                Content.Insert(index, newRow);
            }

            return this;
        }

        public IEasyCsv InsertRecord(List<string> rowValues, int index = -1)
        {
            var headers = GetHeaders();
            if (Content == null) return this;
            if (headers?.Count == null || headers.Count == 0 || headers.Count != rowValues.Count) return this;
            if (index == -1 || index >= Content.Count)
            {
                var newRow = RowFromHeadersAndValues(headers, rowValues);
                Content.Add(newRow);
            }
            else
            {
                var newRow = RowFromHeadersAndValues(headers, rowValues);
                Content.Insert(index, newRow);
            }

            return this;
        }

        private static Dictionary<string, object> RowFromHeadersAndValues(IEnumerable<string> headers, List<string> values)
        {
            var row = new Dictionary<string, object>();
            var i = 0;
            foreach (var key in headers)
            {
                row[key] = values.ElementAt(i);
                i++;
            }
            return row;
        }

        public IEasyCsv UpsertRecord(IDictionary<string, object> row)
        {
            if (Content == null) return this;
            var index = Content.FindIndex(r => RowsEqual(r, row));
            if (index >= 0)
            {
                Content[index] = row;
            }
            else
            {
                Content.Add(row);
            }

            return this;
        }

        public IEasyCsv UpsertRecords(IEnumerable<IDictionary<string, object>> rows)
        {
            foreach (var row in rows)
            {
                UpsertRecord(row);
            }

            return this;
        }

        public IDictionary<string, object>? GetRecord(int index)
        {
            if (Content == null || index < 0 || index >= Content.Count)
            {
                return null;
            }

            return Content[index];
        }

        public T? GetRecord<T>(int index) where T : class
        {
            if (Content == null || index < 0 || index >= Content.Count)
            {
                return null;
            }
            var row = GetRecord(index);
            if (row == null) return null;
            var content = new List<IDictionary<string, object>>() { row };
            var easyCsv = new EasyCsv(content, Config);
            if(easyCsv.ContentBytes == null) return null;
            using var reader = new StreamReader(new MemoryStream(easyCsv.ContentBytes), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, Config.CsvHelperConfig);
            return csvReader.GetRecord<T>();
        }

        public IEasyCsv UpdateRecord(int index, IDictionary<string, object> newRow)
        {
            if (Content == null || index < 0 || index >= Content.Count)
            {
                return this;
            }

            Content[index] = newRow;
            return this;
        }

        public IEasyCsv DeleteRecord(int index)
        {
            if (Content == null || index < 0 || index >= Content.Count)
            {
                return this;
            }

            Content.RemoveAt(index);
            return this;
        }
    }
}