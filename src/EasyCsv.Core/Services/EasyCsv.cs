using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

[assembly: InternalsVisibleTo("EasyCsv.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Core")]
namespace EasyCsv.Core
{
    internal class EasyCsv : ICsvService
    {
        internal bool NormalizeFields { get; set; }
        public byte[]? FileContentBytes { get; set; }
        public string? FileContentStr { get; set; }
        public List<IDictionary<string, object>>? CsvContent { get; private set; }

        internal EasyCsv() { }
            
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

        /// <summary>
        /// Gets the headers of the CSV file.
        /// </summary>
        /// <returns>An IEnumerable of string containing the CSV headers.</returns>
        public List<string>? GetHeaders()
        {
            return CsvContent?.FirstOrDefault()?.Keys.ToList();
        }

        public void ReplaceHeaderRow(List<string> newHeaderFields)
        {
            if (CsvContent == null) return;
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
        }

        /// <summary>
        /// Removes the column of the old header field and upserts all it's values to all the rows of the new header field. CSV.
        /// </summary>
        /// <param name="oldHeaderField">The column that will be removed.</param>
        /// <param name="newHeaderField">The column that will contain all the values of the old column.</param>
        
        public void ReplaceColumn(string oldHeaderField, string newHeaderField)
        {
            if (CsvContent == null) return;

            var normalizedNewValue = Normalize(newHeaderField);
            foreach (var row in CsvContent)
            {
                if (!row.TryGetValue(oldHeaderField, out var temp)) continue;
                row.Remove(oldHeaderField);
                row[normalizedNewValue] = temp;
            }
        }

        /// <summary>
        /// Adds or replaces all the values for multiples columns in the CSV.
        /// </summary>
        /// <param name="defaultValues">Header Field, Default Value. Dictionary of the header fields of the columns you want to give a default value to.</param>
        /// /// <param name="upsert">Determines whether or not an exception is thrown if the column already exists.</param>

        public void CreateColumnWithDefaultValue(Dictionary<string, string> defaultValues, bool upsert = true)
        {
            if (CsvContent == null) return;

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
        }

        /// <summary>
        /// Adds or replaces all the values for a given column in the CSV.
        /// </summary>
        /// <param name="header">The header field of the column you are giving a default value to.</param>
        /// <param name="value">The value you want every record in a column to have.</param>

        public void CreateColumnsWithDefaultValue(string header, string value, bool upsert = true)
        {
            if (CsvContent == null) return;

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
        }

        /// <summary>
        /// Removes rows from the CSV content that don't match the predicate.
        /// </summary>
        public void FilterRows(Func<IDictionary<string, object>, bool> predicate)
        {
            CsvContent = CsvContent?.Where(predicate).ToList();
        }


        /// <summary>
        /// Replace values in a column based on the valueMapping dictionary
        /// </summary>
        /// <param name="headerField">The header field of the column to do value mapping.</param>
        public void MapValuesInColumn(string headerField, Dictionary<object, object> valueMapping)
        {
            if (CsvContent == null) return;

            foreach (var row in CsvContent)
            {
                if (row.ContainsKey(headerField) && valueMapping.ContainsKey(row[headerField]))
                {
                    row[headerField] = valueMapping[row[headerField]];
                }
            }
        }

        /// <summary>
        /// Sorts the csv based on provided header field.
        /// </summary>
        /// <param name="headerField">The header field that you would like to use as the basis of the sorting.</param>

        public void SortCsvByColumnData(string headerField, bool ascending = true)
        {
            if (CsvContent == null) return;

            CsvContent = ascending
                ? CsvContent.OrderBy(row => row[headerField]).ToList()
                : CsvContent.OrderByDescending(row => row[headerField]).ToList();
        }

        /// <summary>
        /// Sorts the csv based on provided header field.
        /// </summary>
        /// <param name="headerField">The header field that you would like to use as the basis of the sorting.</param>

        public void SortCsv(string headerField, Func<IDictionary<string, object>, bool> predicate, bool ascending = true)
        {
            if (CsvContent == null) return;

            CsvContent = ascending
                ? CsvContent.OrderBy(row => row[headerField]).ToList()
                : CsvContent.OrderByDescending(row => row[headerField]).ToList();
        }

        /// <summary>
        /// Sorts the rows in the CSV content based on a custom key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the keySelector function.</typeparam>
        /// <param name="keySelector">A function that defines how to extract a key from each row for comparison.</param>
        /// <param name="ascending">A boolean value indicating whether the rows should be sorted in ascending order. If false, the rows will be sorted in descending order. The default value is true.</param>
        /// <example>
        /// This example sorts the rows by the length of the string in the "FieldName" column in descending order:
        /// <code>
        /// csvService.SortRows(row => row["FieldName"].ToString().Length, ascending: false);
        /// </code>
        /// </example>

        public void SortRows<TKey>(Func<IDictionary<string, object>, TKey> keySelector, bool ascending = true)
        {
            if (CsvContent == null) return;

            CsvContent = ascending
                ? CsvContent.OrderBy(keySelector).ToList()
                : CsvContent.OrderByDescending(keySelector).ToList();
        }

        /// <summary>
        /// Removes columns from the CSV content.
        /// </summary>
        /// <param name="headerFields">The header fields of the columns you want to remove.</param>

        public void RemoveColumns(List<string> headerFields)
        {
            if (CsvContent == null) return;
            if (headerFields.Any(x => !GetHeaders()?.Contains(x) ?? true))
                throw new ArgumentException($"At least one of the columns requested for removal did not exist..");
            foreach (var record in CsvContent)
            {
                foreach (var field in headerFields) 
                {
                    record.Remove(field);
                }
            }
        }

        /// <summary>
        /// Removes a column from the CSV content.
        /// </summary>
        /// <param name="headerField">The header field of the column you want to remove.</param>

        public void RemoveColumn(string headerField)
        {
            if (CsvContent == null) return;
            if (!GetHeaders()?.Contains(headerField) ?? true)
                throw new ArgumentException($"No column existed with headerField: {headerField}.");
            foreach (var record in CsvContent)
            {
                record.Remove(headerField);
            }
        }

        /// <summary>
        /// Calculates the <code>FileContentStr</code> amd <code>FileContentBytes</code>.
        /// </summary>
        
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
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        
        public List<T> GetRecords<T>() where T : new()
        {
            if (CsvContent == null) return new List<T>();

            List<T> resultList = new List<T>(CsvContent.Count);
            Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();
            Dictionary<string, bool> conversionCache = new Dictionary<string, bool>();
            var tProperties = typeof(T).GetProperties();
            foreach (IDictionary<string, object> csvRow in CsvContent)
            {
                var obj = new T();
                foreach (KeyValuePair<string, object> csvField in csvRow)
                {
                    if (!propertyCache.TryGetValue(csvField.Key, out PropertyInfo? property))
                    {
                        var newPropertyCacheItem = tProperties.FirstOrDefault(p => string.Equals(p.Name, csvField.Key, StringComparison.OrdinalIgnoreCase));
                        if (newPropertyCacheItem == null) continue;
                        propertyCache[csvField.Key] = newPropertyCacheItem;
                        property = newPropertyCacheItem;
                    }

                    if (property == null) continue;

                    Type propertyType = property.PropertyType;
                    if (csvField.Value == null) continue;

                    Type entryValueType = csvField.Value.GetType();

                    if (propertyType.IsAssignableFrom(entryValueType))
                    {
                        property.SetValue(obj, csvField.Value);
                        continue;
                    }
                    if (entryValueType != typeof(string)) continue;

                    TypeConverter converter = TypeDescriptor.GetConverter(propertyType);

                    if (conversionCache.TryGetValue(csvField.Key, out bool convertible))
                    {
                        object convertedValue = converter.ConvertFromInvariantString((string)csvField.Value)!;
                        property.SetValue(obj, convertedValue);
                    }
                    else
                    {
                        if (!converter.CanConvertFrom(typeof(string))) continue;

                        try
                        {
                            object convertedValue = converter.ConvertFromInvariantString((string)csvField.Value)!;
                            property.SetValue(obj, convertedValue);
                            conversionCache[csvField.Key] = true;
                        }
                        catch (Exception)
                        {
                            conversionCache[csvField.Key] = false;
                        }
                    }
                }
                resultList.Add(obj);
            }
            return resultList;
        }

        private string Normalize(string header)
        {
            return NormalizeFields ? header.Replace(" ", "").Replace("\"", "").ToLower() : header;
        }
    }
}
