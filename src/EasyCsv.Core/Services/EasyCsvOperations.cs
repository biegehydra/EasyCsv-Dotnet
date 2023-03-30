using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        public IEasyCsv ReplaceHeaderRow(List<string> newHeaderFields)
        {
            if (Content == null) return this;
            var oldHeaders = GetHeaders()?.ToList();
            if (newHeaderFields.Count != oldHeaders?.Count)
            {
                throw new ArgumentException("Replacement header row field count does not match the current header row field count");
            }

            var updatedCsvContent = new List<IDictionary<string, object>>(Content.Count);

            foreach (var row in Content)
            {
                var newRow = new Dictionary<string, object>();
                var i = 0;

                foreach (var oldHeaderField in oldHeaders)
                {
                    var newHeaderField = newHeaderFields.ElementAt(i);
                    newRow[newHeaderField] = row[oldHeaderField];
                    i++;
                }

                updatedCsvContent.Add(newRow);
            }

            Content = updatedCsvContent;
            return this;

        }

        public IEasyCsv ReplaceColumn(string oldHeaderField, string newHeaderField)
        {
            if (Content == null) return this;

            var normalizedNewValue = Normalize(newHeaderField);
            foreach (var row in Content)
            {
                if (!row.TryGetValue(oldHeaderField, out var temp)) continue;
                row.Remove(oldHeaderField);
                row[normalizedNewValue] = temp;
            }
            return this;
        }

        public IEasyCsv AddColumn(string header, string value, bool upsert = true)
        {
            if (Content == null) return this;

            var normalizedDefaultHeader = Normalize(header);
            foreach (var record in Content)
            {
                if (upsert)
                {
                    record[normalizedDefaultHeader] = value;
                    continue;
                }
                if (record.ContainsKey(normalizedDefaultHeader))
                {
                    throw new ArgumentException($"Value for key '{header}' already exists.");
                }
                record.Add(normalizedDefaultHeader, value);
            }
            return this;
        }

        public IEasyCsv AddColumns(Dictionary<string, string> defaultValues, bool upsert = true)
        {
            if (Content == null) return this;

            foreach (var record in Content)
            {
                foreach (var (key, value) in defaultValues)
                {
                    var normalizedDefaultHeader = Normalize(key);
                    if (upsert)
                    {
                        record[normalizedDefaultHeader] = value;
                        continue;
                    }
                    if (record.ContainsKey(normalizedDefaultHeader))
                    {
                        throw new ArgumentException($"Value for key '{key}' already exists.");
                    }
                    record.Add(normalizedDefaultHeader, value);
                }
            }
            return this;
        }

        public IEasyCsv FilterRows(Func<IDictionary<string, object>, bool> predicate)
        {
            Content = Content?.Where(predicate).ToList();
            return this;
        }

        public IEasyCsv MapValuesInColumn(string headerField, Dictionary<object, object> valueMapping)
        {
            if (Content == null) return this;

            foreach (var row in Content)
            {
                if (row.ContainsKey(headerField) && valueMapping.ContainsKey(row[headerField]))
                {
                    row[headerField] = valueMapping[row[headerField]];
                }
            }
            return this;
        }

        public IEasyCsv SortCsv(string headerField, bool ascending = true)
        {
            if (Content == null) return this;

            Content = ascending
                ? Content.OrderBy(row => row[headerField]).ToList()
                : Content.OrderByDescending(row => row[headerField]).ToList();
            return this;
        }

        public IEasyCsv SortCsv<TKey>(Func<IDictionary<string, object>, TKey> keySelector, bool ascending = true)
        {
            if (Content == null) return this;

            Content = ascending
                ? Content.OrderBy(keySelector).ToList()
                : Content.OrderByDescending(keySelector).ToList();
            return this;
        }


        public IEasyCsv RemoveColumn(string headerField)
        {
            if (Content == null) return this;
            if (!GetHeaders()?.Contains(headerField) ?? true)
                throw new ArgumentException($"No column existed with headerField: {headerField}.");
            foreach (var record in Content)
            {
                record.Remove(headerField);
            }

            return this;
        }

        public IEasyCsv RemoveColumns(List<string> headerFields)
        {
            if (Content == null) return this;
            if (headerFields.Any(x => !GetHeaders()?.Contains(x) ?? true))
                throw new ArgumentException($"At least one of the columns requested for removal did not exist..");
            foreach (var record in Content)
            {
                foreach (var field in headerFields)
                {
                    record.Remove(field);
                }
            }
            return this;
        }


        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(bool caseInsensitive)
        {
            var records = await GetRecordsAsync<T>(caseInsensitive);
            return await FromObjectsAsync<T>(records, Config);
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            var records = await GetRecordsAsync<T>(prepareHeaderForMatch);
            return await FromObjectsAsync(records, Config);
        }

        public async Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig)
        {
            var records = await GetRecordsAsync<T>(csvConfig);
            return await FromObjectsAsync(records, Config);
        }

        public IEasyCsv Clear()
        {
            Content?.Clear();
            return this;
        }


        public IEasyCsv Combine(IEasyCsv? otherCsv)
        {
            if (Content == null) return this;
            if (otherCsv == null || otherCsv.Content == null) return this;

            var firstHeaders = GetHeaders();
            var secondHeaders = otherCsv.GetHeaders();

            if (secondHeaders == null) return this;
            if (!firstHeaders?.SequenceEqual(secondHeaders) ?? true) return this;

            Content.AddRange(otherCsv.Content);
            return this;
        }

        public IEasyCsv Combine(List<IEasyCsv?>? otherCsvs)
        {
            if (otherCsvs == null)
            {

            }
            if (Content == null) return this;
            if (otherCsvs == null || otherCsvs.Count <= 0) return this;
            foreach (var otherCsv in otherCsvs)
            {
                Combine(otherCsv);
            }


            return this;
        }
    }
}
