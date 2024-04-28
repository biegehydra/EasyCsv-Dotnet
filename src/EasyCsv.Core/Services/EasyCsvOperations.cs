using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using EasyCsv.Core.Configuration;
using EasyCsv.Core.Enums;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        public IEasyCsv ReplaceHeaderRow(IReadOnlyList<string> newHeaderFields)
        {
            if (CsvContent == null) return this;
            var oldHeaders = ColumnNames()?.ToArray();
            if (newHeaderFields.Count != oldHeaders?.Length)
            {
                throw new ArgumentException("Replacement header row field count does not match the current header row field count");
            }
            var newRecords = new List<CsvRow>(CsvContent.Count);
            foreach (var row in CsvContent)
            {
                var newRow = new CsvRow(newHeaderFields, row.Values.ToList());
                newRecords.Add(newRow);
            }
            CsvContent = newRecords;
            return this;

        }

        public IEasyCsv ReplaceColumn(string oldHeaderField, string newHeaderField)
        {
            if (CsvContent == null) return this;

            foreach (var row in CsvContent)
            {
                if (!row.TryGetValue(oldHeaderField, out var temp)) continue;
                row.Remove(oldHeaderField);
                row[newHeaderField] = temp;
            }
            return this;
        }

        public IEasyCsv MoveColumn(int oldIndex, int newIndex)
        {
            if (oldIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(oldIndex));
            if (newIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(newIndex));
            var columnNames = ColumnNames();
            if (columnNames == null) return this;
            if (oldIndex < columnNames.Length)
            {
                return MoveColumn(columnNames[oldIndex], newIndex);
            }
            throw new ArgumentException($"Index outside bounds of csv. Headers Count: {columnNames.Length}, Index: {oldIndex}", nameof(oldIndex));
        }

        public IEasyCsv MoveColumn(string columnName, int newIndex)
        {
            if (newIndex < 0)
            {
                throw new ArgumentException("Index cannot be negative", nameof(newIndex));
            }

            var headers = ColumnNames();
            if (CsvContent == null || headers == null) return this;

            var oldIndex = headers.IndexOf(columnName);
            if (oldIndex < 0)
            {
                throw new ArgumentException($"Column doesn't exist. Column Name: `{columnName}`");
            }
            if (oldIndex == newIndex)
            {
                return this;
            }
            var newRowData = new Dictionary<string, object?>();
            bool forward = newIndex > oldIndex;
            foreach (var row in CsvContent)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == oldIndex) continue;
                    if (forward)
                    {
                        newRowData[headers[i]] = row.ValueAt(i);
                    }
                    if (i == newIndex)
                    {
                        newRowData[columnName] = row.ValueAt(oldIndex);
                    }
                    if (!forward)
                    {
                        newRowData[headers[i]] = row.ValueAt(i);
                    }
                }
                row.Clear();
                foreach (var kvp in newRowData)
                {
                    row.Add(kvp.Key, kvp.Value);
                }
                newRowData.Clear();
            }
            return this;
        }

        public IEasyCsv InsertColumn(int index, string columnName, object? defaultValue)
        {
            if (index < 0)
            {
                throw new ArgumentException("Index cannot be negative", nameof(index));
            }
            var headers = ColumnNames();
            if (CsvContent == null || headers == null) return this;
            if (index > headers.Length)
            {
                index = headers.Length;
            }

            var indexOf = headers.IndexOf(columnName);
            if (indexOf != -1 && indexOf != index)
            {
                throw new ArgumentException($"Column with name '{columnName}' already exists.");
            }
            if (indexOf == index)
            {
                return this;
            }
            if (index == headers.Length)
            {
                return AddColumn(columnName, defaultValue);
            }
            var newRowData = new Dictionary<string, object?>();
            foreach (var row in CsvContent)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == index)
                    {
                        newRowData[columnName] = defaultValue;
                    }
                    newRowData[headers[i]] = row.ValueAt(i);
                }
                row.Clear();
                foreach (var kvp in newRowData)
                {
                    row.Add(kvp.Key, kvp.Value);
                }
                newRowData.Clear();
            }
            return this;
        }

        public IEasyCsv AddColumn(string columnName, object? defaultValue, ExistingColumnHandling  existingColumnHandling = ExistingColumnHandling.Override)
        {
            if (CsvContent == null) return this;

            foreach (var record in CsvContent)
            {
                if (existingColumnHandling == ExistingColumnHandling.Override)
                {
                    record[columnName] = defaultValue;
                    continue;
                }
                if (record.ContainsKey(columnName))
                {
                    if (existingColumnHandling == ExistingColumnHandling.Keep) continue;
                    if (existingColumnHandling == ExistingColumnHandling.ReplaceNullOrWhiteSpace)
                    {
                        if (string.IsNullOrWhiteSpace(record[columnName]?.ToString()))
                        {

                        }
                        continue;
                    }
                    if (existingColumnHandling == ExistingColumnHandling.ThrowException)
                    {
                        throw new ArgumentException($"Column with name '{columnName}' already exists.");
                    }
                    else throw new ArgumentException("Unreachable");
                }
                record.Add(columnName, defaultValue);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultValues"></param>
        /// <param name="upsert">
        /// True - Will override if column exists
        /// Null - Will continue if column exists
        /// False - Will throw if column exists
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IEasyCsv AddColumns(IDictionary<string, object?> defaultValues, ExistingColumnHandling existingColumnHandling = ExistingColumnHandling.Override)
        {
            if (CsvContent == null) return this;

            foreach (var record in CsvContent)
            {
                foreach (KeyValuePair<string, object?> pair in defaultValues)
                {
                    string key = pair.Key;
                    if (existingColumnHandling == ExistingColumnHandling.Override)
                    {
                        record[key] = pair.Value;
                        continue;
                    }
                    if (record.ContainsKey(key))
                    {
                        if (existingColumnHandling == ExistingColumnHandling.Keep) continue;
                        if (existingColumnHandling == ExistingColumnHandling.ReplaceNullOrWhiteSpace)
                        {
                            if (string.IsNullOrWhiteSpace(record[key]?.ToString()))
                            {

                            }
                            continue;
                        }
                        if (existingColumnHandling == ExistingColumnHandling.ThrowException)
                        {
                            throw new ArgumentException($"Column with name '{key}' already exists.");
                        }
                        else throw new ArgumentException("Unreachable");
                    }
                    record.Add(key, pair.Value);
                }
            }
            return this;
        }

        public IEasyCsv FilterRows(Func<CsvRow, bool> predicate)
        {
            CsvContent = CsvContent?.Where(predicate).ToList();
            return this;
        }

        public IEasyCsv MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping)
        {
            if (CsvContent == null) return this;

            foreach (var row in CsvContent)
            {
                if (row.TryGetValue(headerField, out var oldValue) && oldValue != null && valueMapping.TryGetValue(oldValue, out var newValue))
                {
                    row[headerField] = newValue;
                }
            }
            return this;
        }

        public IEasyCsv SortCsv(string headerField, bool ascending = true)
        {
            if (CsvContent == null) return this;

            CsvContent = ascending
                ? CsvContent.OrderBy(row => row[headerField]).ToList()
                : CsvContent.OrderByDescending(row => row[headerField]).ToList();
            return this;
        }

        public IEasyCsv SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true)
        {
            if (CsvContent == null) return this;

            CsvContent = ascending
                ? CsvContent.OrderBy(keySelector).ToList()
                : CsvContent.OrderByDescending(keySelector).ToList();
            return this;
        }


        public IEasyCsv RemoveColumn(string headerField, bool throwIfNotExists = true)
        {
            if (CsvContent == null) return this;
            if (!ColumnNames()?.Contains(headerField) ?? true)
            {
                if (throwIfNotExists)
                {
                    throw new ArgumentException($"No column existed with headerField: {headerField}.");
                }
                return this;

            }
            foreach (var record in CsvContent)
            {
                record.Remove(headerField);
            }

            return this;
        }

        public IEasyCsv RemoveColumns(IEnumerable<string> headerFields, bool throwIfNotExists = true)
        {
            if (CsvContent == null) return this;
            string[] headerFieldsArr = headerFields.ToArray();
            if (headerFieldsArr.Any(x => !ColumnNames()?.Contains(x) ?? true))
            {
                if (throwIfNotExists)
                {
                    throw new ArgumentException($"At least one of the columns requested for removal did not exist..");
                }
                headerFieldsArr = headerFieldsArr.Where(x => ColumnNames()?.Contains(x) == true).ToArray();
            }

            if (headerFieldsArr.Length != 0)
            {
                foreach (var record in CsvContent)
                {
                    foreach (var field in headerFieldsArr)
                    {
                        record.Remove(field);
                    }
                }
            }
            return this;
        }

        public IEasyCsv RemoveUnusedHeaders()
        {
            if (CsvContent == null) return this;
            var keys = ColumnNames();
            if (keys == null) return this;
            foreach (var key in keys)
            {
                if (CsvContent.All(x => (x[key] is string str && string.IsNullOrWhiteSpace(str)) || x[key] == null))
                {
                    RemoveColumn(key);
                }
            }
            return this;
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(bool caseInsensitive, CsvContextProfile? csvContextProfile = null)
        {
            var records = await GetRecordsAsync<T>(caseInsensitive);
            return await FromObjectsAsync<T>(records, Config, csvContextProfile);
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null)
        {
            var records = await GetRecordsAsync<T>(prepareHeaderForMatch);
            return await FromObjectsAsync(records, Config, csvContextProfile);
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null)
        {
            var records = await GetRecordsAsync<T>(csvConfig);
            return await FromObjectsAsync(records, Config, csvContextProfile);
        }

        public IEasyCsv SwapColumns(int columnOneIndex, int columnTwoIndex)
        {
            var headers = ColumnNames();
            if (CsvContent == null || headers is not {Length: >= 2} || columnOneIndex == columnTwoIndex) return this;
            if (columnOneIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(columnOneIndex));
            if (columnTwoIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(columnOneIndex));
            var headersCount = headers.Length;
            if (columnOneIndex > headersCount-1)
                throw new ArgumentException($"Index outside bounds. columnOneIndex: {columnOneIndex}, Headers Count: {headersCount}", nameof(columnOneIndex));
            if (columnTwoIndex > headersCount - 1)
                throw new ArgumentException($"Index outside bounds. columnTwoIndex: {columnTwoIndex}, Headers Count: {headersCount}", nameof(columnTwoIndex));
            var columnOneName = headers[columnOneIndex];
            var columnTwoName = headers[columnTwoIndex];
            var reorderedRowData = new Dictionary<string, object?>();
            foreach (var row in CsvContent)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == columnOneIndex)
                    {
                        reorderedRowData[columnTwoName] = row[columnTwoName];
                    }
                    else if (i == columnTwoIndex)
                    {
                        reorderedRowData[columnOneName] = row[columnOneName];
                    }
                    else
                    {
                        reorderedRowData[headers[i]] = row.ValueAt(i);
                    }
                }
                row.Clear();
                foreach (var kvp in reorderedRowData)
                {
                    row.Add(kvp.Key, kvp.Value);
                }
                reorderedRowData.Clear();
            }
            return this;
        }

        public IEasyCsv SwapColumns(string columnOneName, string columnTwoName, IEqualityComparer<string>? comparer = null)
        {
            var headers = ColumnNames();
            if (CsvContent == null || headers is not { Length: >= 2 } || columnOneName == columnTwoName) return this;
            if (string.IsNullOrWhiteSpace(columnOneName))
                throw new ArgumentException("Column name cannot be null or whitespace", nameof(columnOneName));
            if (string.IsNullOrWhiteSpace(columnTwoName))
                throw new ArgumentException("Column name cannot be null or whitespace", nameof(columnTwoName));
            if (!headers.Contains(columnOneName, comparer))
                throw new ArgumentException($"Column {columnOneName} does not exist", nameof(columnOneName));
            if (!headers.Contains(columnTwoName, comparer))
                throw new ArgumentException($"Column {columnTwoName} does not exist", nameof(columnTwoName));

            var columnOneIndex = headers.IndexOf(columnOneName, comparer);
            var columnTwoIndex = headers.IndexOf(columnTwoName, comparer);
            SwapColumns(columnOneIndex, columnTwoIndex);
            return this;
        }

        public IEasyCsv Clear()
        {
            CsvContent?.Clear();
            return this;
        }


        public IEasyCsv Combine(IEasyCsv? otherCsv)
        {
            if (CsvContent == null) return this;
            if (otherCsv?.CsvContent == null) return this;

            var firstHeaders = ColumnNames();
            var secondHeaders = otherCsv.ColumnNames();

            if (secondHeaders == null) return this;
            if (!firstHeaders?.SequenceEqual(secondHeaders) ?? true) return this;

            CsvContent.AddRange(otherCsv.CsvContent);
            return this;
        }

        public IEasyCsv Combine(List<IEasyCsv?>? otherCsvs)
        {
            if (CsvContent == null) return this;
            if (otherCsvs is not {Count: > 0}) return this;
            foreach (var otherCsv in otherCsvs)
            {
                Combine(otherCsv);
            }


            return this;
        }
    }
}
