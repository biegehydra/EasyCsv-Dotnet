using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EasyCsv.Core.Configuration;

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
        public List<CsvRow> CsvContent => _csv.CsvContent;

        public int GetRowCount()
        {

            return _csv.GetRowCount();
        }

        public bool ContainsColumn(string column, bool caseInsensitive)
        {
            return _csv.ContainsColumn(column, caseInsensitive);
        }

        public bool ContainsAllColumns(IEnumerable<string> columns, IEqualityComparer<string>? comparer = null)
        {
            return _csv.ContainsAllColumns(columns, comparer);
        }


        public bool ContainsRow(CsvRow row)
        {
            return _csv.ContainsRow(row);
        }

        public List<string>? GetColumns()
        {
            return _csv.GetColumns();
        }

        public Task<List<T>> GetRecordsAsync<T>(bool strict = false, CsvContextProfile? csvContextProfile = null)
        {
            return _csv.GetRecordsAsync<T>(strict, csvContextProfile);
        }

        public Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null)
        {
            return _csv.GetRecordsAsync<T>(prepareHeaderForMatch, csvContextProfile);
        }

        public Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null)
        {
            return _csv.GetRecordsAsync<T>(csvConfig, csvContextProfile);
        }

        public IAsyncEnumerable<T> ReadRecordsAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null)
        {
            return _csv.ReadRecordsAsync<T>(csvConfig, csvContextProfile);
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

        public IEasyMutations AddColumn(string header, object? value, bool upsert = true)
        {
            _csv.AddColumn(header, value, upsert);
            return this;
        }

        public IEasyMutations MoveColumn(int oldIndex, int newIndex)
        {
            _csv.MoveColumn(oldIndex, newIndex);
            return this;
        }

        public IEasyMutations MoveColumn(string columnName, int newIndex)
        {
            _csv.MoveColumn(columnName, newIndex);
            return this;
        }

        public IEasyMutations InsertColumn(int index, string columnName, object? defaultValue)
        {
            _csv.InsertColumn(index, columnName, defaultValue);
            return this;
        }

        public IEasyMutations AddColumns(IDictionary<string, object?> defaultValues, bool? upsert = true)
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

        public IEasyMutations RemoveColumn(string column, bool throwIfNotExists = true)
        {
            _csv.RemoveColumn(column, throwIfNotExists);
            return this;
        }

        public IEasyMutations RemoveColumns(IEnumerable<string> columns, bool throwIfNotExists = true)
        {
            _csv.RemoveColumns(columns, throwIfNotExists);
            return this;
        }

        public IEasyMutations RemoveUnusedHeaders()
        { 
            _csv.RemoveUnusedHeaders();
            return this;
        }

        public async Task<IEasyMutations> RemoveUnusedHeadersAsync<T>(bool strict = true, CsvContextProfile? csvContextProfile = null)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(strict, csvContextProfile);
            return this;
        }

        public async Task<IEasyMutations> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(prepareHeaderForMatch, csvContextProfile);
            return this;
        }

        public async Task<IEasyMutations> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null)
        {
            await _csv.RemoveUnusedHeadersAsync<T>(csvConfig, csvContextProfile);
            return this;
        }

        public IEasyMutations SwapColumns(int columnOneIndex, int columnTwoIndex)
        {
            _csv.SwapColumns(columnOneIndex, columnTwoIndex);
            return this;
        }

        public IEasyMutations SwapColumns(string columnOneName, string columnTwoName, IEqualityComparer<string>? comparer = null)
        {
            _csv.SwapColumns(columnOneName, columnTwoName, comparer);
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

        public IEasyMutations InsertRecord(List<object?> rowValues, int index = -1)
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