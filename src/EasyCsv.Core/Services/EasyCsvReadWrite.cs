using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

        internal static IEasyCsv FromObjects<T>(IEnumerable<T> objects, EasyCsvConfiguration config)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig))
            {
                csvWriter.WriteRecords(objects);
                csvWriter.Flush();
            }

            using var memoryStream2 = new MemoryStream(memoryStream.ToArray());
            using var streamReader = new StreamReader(memoryStream2);
            var csvContent = streamReader.ReadToEnd();

            return new EasyCsv(csvContent, config);
        }
        internal static async Task<IEasyCsv> FromObjectsAsync<T>(IEnumerable<T> objects, EasyCsvConfiguration config)
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

        public async Task<List<T>> GetRecordsAsync<T>(bool caseInsensitive = false)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;

            if (caseInsensitive)
            {
                await ReadRecordsStrict(records);
            }
            else
            {
                await ReadRecordsNotStrict(records);
            }
            return records;

            async Task ReadRecordsStrict(List<T> records)
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, Config.CsvHelperConfig);
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
            async Task ReadRecordsNotStrict(List<T> records)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.ToLower(CultureInfo.InvariantCulture)
                };
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, config);
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;
            await ReadRecords(records, prepareHeaderForMatch);
            return records;

            async Task ReadRecords(List<T> records, PrepareHeaderForMatch prepareHeaderForMatch)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = prepareHeaderForMatch,
                };
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, config);
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;
            await ReadRecords(records, csvConfig);
            return records;

            async Task ReadRecords(ICollection<T> records, CsvConfiguration csvConfig)
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, csvConfig);
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async IAsyncEnumerable<T> ReadRecordsAsync<T>(CsvConfiguration config)
        {
            using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, config);
            await foreach (var record in csvReader.GetRecordsAsync<T>())
            {
                yield return record;
            }
        }
    }
}
