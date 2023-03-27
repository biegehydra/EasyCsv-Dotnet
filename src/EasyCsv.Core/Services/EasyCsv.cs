using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

[assembly: InternalsVisibleTo("EasyCsv.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Core")]
namespace EasyCsv.Core
{
    internal class EasyCsv : ICsvService
    {
        private bool NormalizeFields { get; set; }
        public byte[]? FileContentBytes { get; set; }
        public string? FileContentStr { get; set; }
        public List<IDictionary<string, object>>? CsvContent { get; private set; }

        internal EasyCsv(bool normalizeFields = false)
        {
            NormalizeFields = normalizeFields;
        }
            
        public EasyCsv(byte[] fileContent, bool normalizeFields = false)
        {
            FileContentBytes = fileContent;
            FileContentStr = Encoding.UTF8.GetString(FileContentBytes);
            NormalizeFields = normalizeFields;
            CreateCsvContent();
        }

        public EasyCsv(string fileContent, bool normalizeFields = false)
            : this( Encoding.UTF8.GetBytes(fileContent), normalizeFields)
        {

        }

        public EasyCsv(Stream fileContentStream, int maxFileSize, bool normalizeFields = false)
            : this(ReadStreamToEnd(fileContentStream, maxFileSize), normalizeFields)
        {
        }

        static byte[] ReadStreamToEnd(Stream stream, int maxFileSize)
        {
            using var memoryStream = new MemoryStream(maxFileSize);
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public EasyCsv(TextReader fileContentReader, bool normalizeFields = false)
            : this(fileContentReader.ReadToEnd(), normalizeFields)
        {

        }

        internal void CreateCsvContent()
        {
            if (FileContentBytes == null) return;
            using (var reader = new StreamReader(new MemoryStream(FileContentBytes), Encoding.Default))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                CsvContent = csv.GetRecords<dynamic>().Select(x => (IDictionary<string, object>)x).ToList();
            }
        }

        internal async Task CreateCsvContentInBackGround()
        {
            if (FileContentBytes == null) return;
            await Task.Run(() =>
            {
                using (var reader = new StreamReader(new MemoryStream(FileContentBytes), Encoding.Default))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    CsvContent = csv.GetRecords<dynamic>().Select(x => (IDictionary<string, object>)x).ToList();
                }
            });
        }


        public List<string>? GetHeaders()
        {
            return CsvContent?.FirstOrDefault()?.Keys.ToList();
        }

        public ICsvService ReplaceHeaderRow(List<string> newHeaderFields)
        {
            if (CsvContent == null) return this;
            var oldHeaders = GetHeaders()?.ToList();
            if (newHeaderFields.Count() != oldHeaders?.Count())
            {
                throw new ArgumentException("Replacement header row field count does not match the current header row field count");
            }

            var updatedCsvContent = new List<IDictionary<string, object>>(CsvContent.Count);

            foreach (var row in CsvContent)
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

            CsvContent = updatedCsvContent;
            return this;

        }

        public ICsvService ReplaceColumn(string oldHeaderField, string newHeaderField)
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

        public ICsvService AddColumn(string header, string value, bool upsert = true)
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

        public ICsvService AddColumns(Dictionary<string, string> defaultValues, bool upsert = true)
        {
            if (CsvContent == null) return this;

            foreach (var record in CsvContent)
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

        public ICsvService FilterRows(Func<IDictionary<string, object>, bool> predicate)
        {
            CsvContent = CsvContent?.Where(predicate).ToList();
            return this;
        }

        public ICsvService MapValuesInColumn(string headerField, Dictionary<object, object> valueMapping)
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

        public ICsvService SortCsv(string headerField, bool ascending = true)
        {
            if (CsvContent == null) return this;

            CsvContent = ascending
                ? CsvContent.OrderBy(row => row[headerField]).ToList()
                : CsvContent.OrderByDescending(row => row[headerField]).ToList();
            return this;
        }

        public ICsvService SortRows<TKey>(Func<IDictionary<string, object>, TKey> keySelector, bool ascending = true)
        {
            if (CsvContent == null) return this;

            CsvContent = ascending
                ? CsvContent.OrderBy(keySelector).ToList()
                : CsvContent.OrderByDescending(keySelector).ToList();
            return this;
        }

        public ICsvService RemoveColumns(List<string> headerFields)
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


        public ICsvService RemoveColumn(string headerField)
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

        
        public async Task CalculateFileContent()
        {
            if (CsvContent == null) return;
#if NET7_0
            await using var writer = new StringWriter();
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
#else
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
#endif
            {
                var dynamicContent = CsvContent.Cast<dynamic>();
                await csv.WriteRecordsAsync(dynamicContent);
                var str = writer.ToString();
                FileContentBytes = Encoding.UTF8.GetBytes(str);
                FileContentStr = str;
            }
        }

        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="strict">Determine whether property matching is case sensitive</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        public async Task<List<T>> GetRecords<T>(bool strict = false)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateFileContent();
            if (string.IsNullOrEmpty(FileContentStr)) return records;

            if (strict)
            {
                await ReadRecordsStrict(records);
            }
            else
            {
                await ReadRecordsNotStrict(records);
            }
            return records;
        }

        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="prepareHeaderForMatch">Determine whether property matching is case sensitive</param>
        /// <example><code>
        /// PrepareHeaderForMatch = args => args.Header.ToLower();
        /// </code></example>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        public async Task<List<T>> GetRecords<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateFileContent();
            if (string.IsNullOrEmpty(FileContentStr)) return records;
            await ReadRecordsCustom(records, prepareHeaderForMatch);
            return records;
        }

        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="csvConfig">The configuration for reading the csv into records</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        public async Task<List<T>> GetRecords<T>(CsvConfiguration csvConfig)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateFileContent();
            if (string.IsNullOrEmpty(FileContentStr)) return records;
            await ReadRecordsCustom(records, csvConfig);
            return records;
        }

        private async Task ReadRecordsStrict<T>(List<T> records)
        {
            using (var reader = new StreamReader(new MemoryStream(FileContentBytes!), Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }
        private async Task ReadRecordsNotStrict<T>(List<T> records)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };
            using (var reader = new StreamReader(new MemoryStream(FileContentBytes!), Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        private async Task ReadRecordsCustom<T>(List<T> records, PrepareHeaderForMatch prepareHeaderForMatch)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = prepareHeaderForMatch,
            };
            using (var reader = new StreamReader(new MemoryStream(FileContentBytes!), Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }
        private async Task ReadRecordsCustom<T>(List<T> records, CsvConfiguration csvConfig)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };
            using (var reader = new StreamReader(new MemoryStream(FileContentBytes!), Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        private string Normalize(string header)
        {
            return NormalizeFields ? header.Replace(" ", "").Replace("\"", "").ToLower() : header;
        }
    }
}
