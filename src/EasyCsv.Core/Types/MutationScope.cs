using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace EasyCsv.Core
{
    public class CSVMutationScope : IEasyMutations
    {
        private readonly IEasyCsv _csv;

        public CSVMutationScope(IEasyCsv csv)
        {
            _csv = csv;
        }

        public byte[]? ContentBytes => _csv.ContentBytes;
        public string? ContentStr => _csv.ContentStr;
        public List<CsvRow>? CsvContent => _csv.CsvContent;

        public int GetRowCount()
        {

            return _csv.GetRowCount();
        }

        public bool ContainsHeader(string headerField, bool caseInsensitive)
        {
            return _csv.ContainsHeader(headerField, caseInsensitive);
        }

        public bool ContainsRow(CsvRow row)
        {
            return _csv.ContainsRow(row);
        }

        public List<string>? GetHeaders()
        {
            return _csv.GetHeaders();
        }

        public Task<List<T>> GetRecordsAsync<T>(bool strict = false)
        {
            return _csv.GetRecordsAsync<T>(strict);
        }

        public Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            return _csv.GetRecordsAsync<T>(prepareHeaderForMatch);
        }

        public Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig)
        {
            return _csv.GetRecordsAsync<T>(csvConfig);
        }

        public IEasyMutations Clone()
        {
            return new CSVMutationScope(_csv.Clone());
        }

        public IEasyMutations ReplaceHeaderRow(List<string> newHeaderFields)
        {
            _csv.ReplaceHeaderRow(newHeaderFields);
            return this;
        }

        public IEasyMutations ReplaceColumn(string oldHeaderField, string newHeaderField)
        {
            _csv.ReplaceColumn(oldHeaderField, newHeaderField);
            return this;
        }

        public IEasyMutations AddColumn(string header, string value, bool upsert = true)
        {
            _csv.AddColumn(header, value, upsert);
            return this;
        }

        public IEasyMutations AddColumns(IDictionary<string, string> defaultValues, bool upsert = true)
        {
            _csv.AddColumns(defaultValues, upsert);
            return this;
        }

        public IEasyMutations FilterRows(Func<CsvRow, bool> predicate)
        {
            _csv.FilterRows(predicate);
            return this;
        }

        public IEasyMutations MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping)
        {
            _csv.MapValuesInColumn(headerField, valueMapping);
            return this;
        }

        public IEasyMutations SortCsv(string headerField, bool ascending = true)
        {
            _csv.SortCsv(headerField, ascending);
            return this;
        }

        public IEasyMutations SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true)
        {
            _csv.SortCsv(keySelector, ascending);
            return this;
        }

        public IEasyMutations RemoveColumn(string headerField)
        {
            _csv.RemoveColumn(headerField);
            return this;
        }

        public IEasyMutations RemoveColumns(List<string> headerFields)
        {
            _csv.RemoveColumns(headerFields);
            return this;
        }

        public async Task<IEasyMutations> RemoveUnusedHeadersAsync<T>(bool caseInsensitive = true)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(caseInsensitive);
            return this;
        }

        public async Task<IEasyMutations> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(prepareHeaderForMatch);
            return this;
        }

        public async Task<IEasyMutations> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(csvConfig);
            return this;
        }

        public IEasyMutations Clear()
        {
            _csv.Clear();
            return this;
        }

        public IEasyMutations Combine(IEasyCsv? otherCsv)
        {
            if (otherCsv != null)
            {
                _csv.Combine(otherCsv);
            }
            return this;
        }

        public IEasyMutations Combine(List<IEasyCsv?>? otherCsvs)
        {
            if (otherCsvs != null)
            {
                _csv.Combine(otherCsvs);
            }
            return this;
        }

        public IEasyMutations AddRecord(List<string> rowValues, int index = -1)
        {
            _csv.AddRecord(rowValues, index);
            return this;
        }

        public IEasyMutations InsertRecord(List<string> rowValues, int index = -1)
        {
            _csv.InsertRecord(rowValues, index);
            return this;
        }

        public IEasyMutations UpsertRecord(CsvRow row, int index = -1)
        {
            _csv.UpsertRecord(row, index);
            return this;
        }

        public IEasyMutations UpsertRecords(IEnumerable<CsvRow> rows)
        {
            _csv.UpsertRecords(rows);
            return this;
        }

        public CsvRow? GetRecord(int index)
        {
            return _csv.GetRecord(index);
        }

        public T? GetRecord<T>(int index) where T : class
        {
            return _csv.GetRecord<T>(index);
        }

        public IEasyMutations UpdateRecord(int index, CsvRow newRow)
        {
             _csv.UpdateRecord(index, newRow);
             return this;
        }

        public IEasyMutations DeleteRecord(int index)
        {
            _csv.DeleteRecord(index);
            return this;
        }
    }
}