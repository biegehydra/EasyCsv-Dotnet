using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace EasyCsv.Core
{
    public class CSVMutationScope : IEasyCsvBase, IEasyMutations, IGetRecords
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

        public async Task<List<T>> GetRecordsAsync<T>(bool caseInsensitive = false)
        {
            return await _csv.GetRecordsAsync<T>(caseInsensitive);
        }

        public async Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            return await _csv.GetRecordsAsync<T>(prepareHeaderForMatch);
        }

        public async Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig)
        {
            return await _csv.GetRecordsAsync<T>(csvConfig);
        }

        public IEasyCsv Clone()
        {
            return _csv.Clone();
        }

        public IEasyCsv ReplaceHeaderRow(List<string> newHeaderFields)
        {
            _csv.ReplaceHeaderRow(newHeaderFields);
            return _csv;
        }

        public IEasyCsv ReplaceColumn(string oldHeaderField, string newHeaderField)
        {
            _csv.ReplaceColumn(oldHeaderField, newHeaderField);
            return _csv;
        }

        public IEasyCsv AddColumn(string header, string value, bool upsert = true)
        {
            _csv.AddColumn(header, value, upsert);
            return _csv;
        }

        public IEasyCsv AddColumns(IDictionary<string, string> defaultValues, bool upsert = true)
        {
            _csv.AddColumns(defaultValues, upsert);
            return _csv;
        }

        public IEasyCsv FilterRows(Func<CsvRow, bool> predicate)
        {
            _csv.FilterRows(predicate);
            return _csv;
        }

        public IEasyCsv MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping)
        {
            _csv.MapValuesInColumn(headerField, valueMapping);
            return _csv;
        }

        public IEasyCsv SortCsv(string headerField, bool ascending = true)
        {
            _csv.SortCsv(headerField, ascending);
            return _csv;
        }

        public IEasyCsv SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true)
        {
            _csv.SortCsv(keySelector, ascending);
            return _csv;
        }

        public IEasyCsv RemoveColumn(string headerField)
        {
            _csv.RemoveColumn(headerField);
            return _csv;
        }

        public IEasyCsv RemoveColumns(List<string> headerFields)
        {
            _csv.RemoveColumns(headerFields);
            return _csv;
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(bool caseInsensitive = true)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(caseInsensitive);
            return _csv;
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(prepareHeaderForMatch);
            return _csv;
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(csvConfig);
            return _csv;
        }

        public IEasyCsv Clear()
        {
            _csv.Clear();
            return _csv;
        }

        public IEasyCsv Combine(IEasyCsv? otherCsv)
        {
            if (otherCsv != null)
            {
                _csv.Combine(otherCsv);
            }
            return _csv;
        }

        public IEasyCsv Combine(List<IEasyCsv?>? otherCsvs)
        {
            if (otherCsvs != null)
            {
                _csv.Combine(otherCsvs);
            }
            return _csv;
        }

        public IEasyCsv AddRecord(List<string> rowValues, int index = -1)
        {
            _csv.AddRecord(rowValues, index);
            return _csv;
        }

        public IEasyCsv InsertRecord(List<string> rowValues, int index = -1)
        {
            _csv.InsertRecord(rowValues, index);
            return _csv;
        }

        public IEasyCsv UpsertRecord(CsvRow row, int index = -1)
        {
            _csv.UpsertRecord(row, index);
            return _csv;
        }

        public IEasyCsv UpsertRecords(IEnumerable<CsvRow> rows)
        {
            _csv.UpsertRecords(rows);
            return _csv;
        }

        public CsvRow? GetRecord(int index)
        {

            return _csv.GetRecord(index);
        }

        public T? GetRecord<T>(int index) where T : class
        {
            return _csv.GetRecord<T>(index);
        }

        public IEasyCsv UpdateRecord(int index, CsvRow newRow)
        {
            return _csv.UpdateRecord(index, newRow);
        }

        public IEasyCsv DeleteRecord(int index)
        {
            return _csv.DeleteRecord(index);
        }
    }
}