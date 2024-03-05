using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        public IEasyCsv ReplaceHeaderRow(List<string> newHeaderFields)
        {
            if (CsvContent == null) return this;
            var oldHeaders = GetHeaders()?.ToList();
            if (newHeaderFields.Count != oldHeaders?.Count)
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

            var normalizedNewValue = Normalize(newHeaderField);
            foreach (var row in CsvContent)
            {
                if (!row.TryGetValue(oldHeaderField, out var temp)) continue;
                row.Remove(oldHeaderField);
                row[normalizedNewValue] = temp;
            }
            return this;
        }

        public IEasyCsv MoveColumn(int oldIndex, int newIndex)
        {
            if (oldIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(oldIndex));
            if (newIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(newIndex));
            var headers = GetHeaders();
            if (headers == null) return this;
            if (oldIndex < headers.Count)
            {
                return MoveColumn(headers[oldIndex], newIndex);
            }
            throw new ArgumentException($"Index outside bounds of csv. Headers Count: {headers.Count}, Index: {oldIndex}", nameof(oldIndex));
        }

        public IEasyCsv MoveColumn(string columnName, int newIndex)
        {
            if (newIndex < 0)
            {
                throw new ArgumentException("Index cannot be negative", nameof(newIndex));
            }

            var headers = GetHeaders();
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
            var headers = GetHeaders();
            if (CsvContent == null || headers == null) return this;
            if (index > headers.Count)
            {
                index = headers.Count;
            }
            var normalizedColumnName = Normalize(columnName);
            var indexOf = headers.IndexOf(normalizedColumnName);
            if (indexOf != -1 && indexOf != index)
            {
                throw new ArgumentException($"Column with name '{normalizedColumnName}' already exists.");
            }
            if (indexOf == index)
            {
                return this;
            }
            if (index == headers.Count)
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
                        newRowData[normalizedColumnName] = defaultValue;
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

        public IEasyCsv AddColumn(string columnName, object? defaultValue, bool upsert = true)
        {
            if (CsvContent == null) return this;

            var normalizedHeader = Normalize(columnName);
            foreach (var record in CsvContent)
            {
                if (upsert)
                {
                    record[normalizedHeader] = defaultValue;
                    continue;
                }
                if (record.ContainsKey(normalizedHeader))
                {
                    throw new ArgumentException($"Column with name '{normalizedHeader}' already exists.");
                }
                record.Add(normalizedHeader, defaultValue);
            }
            return this;
        }

        public IEasyCsv AddColumns(IDictionary<string, object?> defaultValues, bool upsert = true)
        {
            if (CsvContent == null) return this;

            foreach (var record in CsvContent)
            {
                foreach (KeyValuePair<string, object?> pair in defaultValues)
                {
                    string key = pair.Key;
                    var normalizedDefaultHeader = Normalize(key);
                    if (upsert)
                    {
                        record[normalizedDefaultHeader] = pair.Value;
                        continue;
                    }
                    if (record.ContainsKey(normalizedDefaultHeader))
                    {
                        throw new ArgumentException($"Value for key '{key}' already exists.");
                    }
                    record.Add(normalizedDefaultHeader, pair.Value);
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
            if (!GetHeaders()?.Contains(headerField) ?? true)
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

        public IEasyCsv RemoveColumns(List<string> headerFields, bool throwIfNotExists = true)
        {
            if (CsvContent == null) return this;
            if (headerFields.Any(x => !GetHeaders()?.Contains(x) ?? true))
            {
                if (throwIfNotExists)
                {
                    throw new ArgumentException($"At least one of the columns requested for removal did not exist..");
                }
                headerFields = headerFields.Where(x => GetHeaders()?.Contains(x) == true).ToList();
            }

            if (headerFields.Count != 0)
            {
                foreach (var record in CsvContent)
                {
                    foreach (var field in headerFields)
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
            var keys = GetHeaders();
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
            var headers = GetHeaders();
            if (CsvContent == null || headers is not {Count: >= 2} || columnOneIndex == columnTwoIndex) return this;
            if (columnOneIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(columnOneIndex));
            if (columnTwoIndex < 0)
                throw new ArgumentException("Index cannot be negative", nameof(columnOneIndex));
            var headersCount = headers.Count;
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
            var headers = GetHeaders();
            if (CsvContent == null || headers is not { Count: >= 2 } || columnOneName == columnTwoName) return this;
            if (string.IsNullOrWhiteSpace(columnOneName))
                throw new ArgumentException("Column name cannot be null or whitespace", nameof(columnOneName));
            if (string.IsNullOrWhiteSpace(columnTwoName))
                throw new ArgumentException("Column name cannot be null or whitespace", nameof(columnTwoName));
            if (!headers.Contains(columnOneName, comparer))
                throw new ArgumentException($"Column {columnOneName} does not exist", nameof(columnOneName));
            if (!headers.Contains(columnTwoName, comparer))
                throw new ArgumentException($"Column {columnTwoName} does not exist", nameof(columnTwoName));

            var columnOneIndex = comparer == null ? headers.IndexOf(columnOneName) : headers.FindIndex(x => comparer.Equals(x, columnOneName));
            var columnTwoIndex = comparer == null ? headers.IndexOf(columnTwoName) : headers.FindIndex(x => comparer.Equals(x, columnTwoName));
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

            var firstHeaders = GetHeaders();
            var secondHeaders = otherCsv.GetHeaders();

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
