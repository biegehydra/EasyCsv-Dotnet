using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        private void CreateCsvContent(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.Default);
            using var csv = new CsvReader(reader, Config.CsvHelperConfig);
            CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object>)x)).ToList();
        }

        internal async Task CreateCsvContentInBackGround(Stream stream)
        {
            await Task.Run(() =>
            {
                using var reader = new StreamReader(stream, Encoding.Default);
                using var csv = new CsvReader(reader, Config.CsvHelperConfig);
                CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object>)x)).ToList();
            });
        }

        internal void CreateCsvContent()
        {
            if (ContentBytes == null) return;
            using var reader = new StreamReader(new MemoryStream(ContentBytes), Encoding.Default);
            using var csv = new CsvReader(reader, Config.CsvHelperConfig);
            CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object>) x)).ToList();
        }

        internal async Task CreateCsvContentInBackGround()
        {
            if (ContentBytes == null) return;
            await Task.Run(() =>
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes), Encoding.Default);
                using var csv = new CsvReader(reader, Config.CsvHelperConfig);
                CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object>) x)).ToList();
            });
        }

        internal static IEasyCsv FromObjects<T>(IEnumerable<T> objects, EasyCsvConfiguration config, CsvContextProfile? csvContextProfile = null)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig))
            {
                AddSettingsToCsvContext<T>(csvWriter.Context, csvContextProfile);

                csvWriter.WriteRecords(objects);
                csvWriter.Flush();
            }

            using var memoryStream2 = new MemoryStream(memoryStream.ToArray());
            using var streamReader = new StreamReader(memoryStream2);
            var csvContent = streamReader.ReadToEnd();

            return new EasyCsv(csvContent, config);
        }
        internal static async Task<IEasyCsv> FromObjectsAsync<T>(IEnumerable<T> objects, EasyCsvConfiguration config, CsvContextProfile? csvContextProfile = null)
        {
            using var memoryStream = new MemoryStream();
#if NETSTANDARD2_1_OR_GREATER
            await using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            await using (var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig))
#else
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig);
#endif
            {
                AddSettingsToCsvContext<T>(csvWriter.Context, csvContextProfile);

                await csvWriter.WriteRecordsAsync(objects);
                await csvWriter.FlushAsync();
            }

            using var memoryStream2 = new MemoryStream(memoryStream.ToArray());
            using var streamReader = new StreamReader(memoryStream2);
            var csvContent = await streamReader.ReadToEndAsync();

            return new EasyCsv(csvContent, config);
        }

        internal async Task CalculateContentBytesAndStrAsync()
        {
#if NETSTANDARD2_1_OR_GREATER
            await using var writer = new StringWriter();
            await using var csvWriter = new CsvWriter(writer, Config.CsvHelperConfig);
#else
            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, Config.CsvHelperConfig);
#endif
            var dynamicContent = CsvContent?.Cast<dynamic>();
            await csvWriter.WriteRecordsAsync(dynamicContent);
            var str = writer.ToString();
            ContentBytes = Encoding.UTF8.GetBytes(str);
            ContentStr = str;
        }

        internal void CalculateContentBytesAndStr()
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, Config.CsvHelperConfig);
            var dynamicContent = CsvContent?.Cast<dynamic>();
            csv.WriteRecords(dynamicContent);
            var str = writer.ToString();
            ContentBytes = Encoding.UTF8.GetBytes(str);
            ContentStr = str;
        }

        public async Task<List<T>> GetRecordsAsync<T>(bool strict = false, CsvContextProfile? csvContextProfile = null)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;

            if (strict)
            {
                await ReadRecordsStrict();
            }
            else
            {
                await ReadRecordsNotStrict();
            }
            return records;

            async Task ReadRecordsStrict()
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, Config.CsvHelperConfig);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
            async Task ReadRecordsNotStrict()
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => Regex.Replace(args.Header, @"\W", "").ToLower(CultureInfo.InvariantCulture)
                };
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, config);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        private static void AddSettingsToCsvContext<T>(CsvContext context, CsvContextProfile? csvContextProfile = null)
        {
            if (csvContextProfile?.TypeConverters != null)
            {
                foreach (var typeConverter in csvContextProfile.TypeConverters)
                {
                    context.TypeConverterCache.AddConverter(typeConverter.Key, typeConverter.Value);
                }
            }

            if (csvContextProfile?.ClassMaps != null)
            {
                foreach (var classMap in csvContextProfile.ClassMaps)
                {
                    context.RegisterClassMap(classMap);
                }
            }

            if (csvContextProfile?.TypeConvertersOptionsDict != null)
            {
                foreach (var typeConverterOption in csvContextProfile.TypeConvertersOptionsDict)
                {
                    context.TypeConverterOptionsCache.AddOptions(typeConverterOption.Key, typeConverterOption.Value);
                }
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;
            await ReadRecords();
            return records;

            async Task ReadRecords()
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = prepareHeaderForMatch,
                };
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, config);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;
            await ReadRecords();
            return records;

            async Task ReadRecords()
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, csvConfig);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async IAsyncEnumerable<T> ReadRecordsAsync<T>(CsvConfiguration config, CsvContextProfile? csvContextProfile = null)
        {
            using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, config);

            AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

            await foreach (var record in csvReader.GetRecordsAsync<T>())
            {
                yield return record;
            }
        }
    }
}
