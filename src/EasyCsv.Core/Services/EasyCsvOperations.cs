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
            if (CsvContent == null) return this;
            var oldHeaders = GetHeaders()?.ToList();
            if (newHeaderFields.Count != oldHeaders?.Count)
            {
                throw new ArgumentException("Replacement header row field count does not match the current header row field count");
            }
            var newRecords = new List<CsvRow>(CsvContent.Count);
            foreach (var row in CsvContent)
            {
                var newRow = new CsvRow(newHeaderFields, row.Values.Select(x => (string) x).ToList());
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

        public IEasyCsv AddColumn(string header, string value, bool upsert = true)
        {
            if (CsvContent == null) return this;

            var normalizedDefaultHeader = Normalize(header);
            foreach (var record in CsvContent)
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

        public IEasyCsv AddColumns(IDictionary<string, string> defaultValues, bool upsert = true)
        {
            if (CsvContent == null) return this;

            foreach (var record in CsvContent)
            {
                foreach (KeyValuePair<string, string> pair in defaultValues)
                {
                    string key = pair.Key;
                    string value = pair.Value;
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
                if (row.ContainsKey(headerField) && valueMapping.ContainsKey(row[headerField]))
                {
                    row[headerField] = valueMapping[row[headerField]];
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


        public IEasyCsv RemoveColumn(string headerField)
        {
            if (CsvContent == null) return this;
            if (!GetHeaders()?.Contains(headerField) ?? true)
                throw new ArgumentException($"No column existed with headerField: {headerField}.");
            foreach (var record in CsvContent)
            {
                record.Remove(headerField);
            }

            return this;
        }

        public IEasyCsv RemoveColumns(List<string> headerFields)
        {
            if (CsvContent == null) return this;
            if (headerFields.Any(x => !GetHeaders()?.Contains(x) ?? true))
                throw new ArgumentException($"At least one of the columns requested for removal did not exist..");
            foreach (var record in CsvContent)
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
            CsvContent?.Clear();
            return this;
        }


        public IEasyCsv Combine(IEasyCsv? otherCsv)
        {
            if (CsvContent == null) return this;
            if (otherCsv == null || otherCsv.CsvContent == null) return this;

            var firstHeaders = GetHeaders();
            var secondHeaders = otherCsv.GetHeaders();

            if (secondHeaders == null) return this;
            if (!firstHeaders?.SequenceEqual(secondHeaders) ?? true) return this;

            CsvContent.AddRange(otherCsv.CsvContent);
            return this;
        }

        public IEasyCsv Combine(List<IEasyCsv?>? otherCsvs)
        {
            if (otherCsvs == null)
            {

            }
            if (CsvContent == null) return this;
            if (otherCsvs == null || otherCsvs.Count <= 0) return this;
            foreach (var otherCsv in otherCsvs)
            {
                Combine(otherCsv);
            }


            return this;
        }
    }
}
