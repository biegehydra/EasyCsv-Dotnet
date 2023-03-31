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
        internal void CreateCsvContent()
        {
            if (ContentBytes == null) return;
            using var reader = new StreamReader(new MemoryStream(ContentBytes), Encoding.Default);
            using var csv = new CsvReader(reader, EasyCsvConfiguration.Instance.CsvHelperConfig);
            CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object>) x)).ToList();
        }

        internal async Task CreateCsvContentInBackGround()
        {
            if (ContentBytes == null) return;
            await Task.Run(() =>
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes), Encoding.Default);
                using var csv = new CsvReader(reader, EasyCsvConfiguration.Instance.CsvHelperConfig);
                CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object>) x)).ToList();
            });
        }

        internal static IEasyCsv FromObjects<T>(IEnumerable<T> objects, EasyCsvConfiguration config)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, EasyCsvConfiguration.Instance.CsvHelperConfig))
            {
                csvWriter.WriteRecords(objects);
            }

            memoryStream.Position = 0;
            using var streamReader = new StreamReader(memoryStream);
            var csvContent = streamReader.ReadToEnd();

            return new EasyCsv(csvContent, config);
        }

        internal static async Task<IEasyCsv> FromObjectsAsync<T>(IEnumerable<T> objects, EasyCsvConfiguration config)
        {
            using var memoryStream = new MemoryStream();
            await using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            await using (var csvWriter = new CsvWriter(streamWriter, EasyCsvConfiguration.Instance.CsvHelperConfig))
            {
                await csvWriter.WriteRecordsAsync(objects);
            }

            memoryStream.Position = 0;
            using var streamReader = new StreamReader(memoryStream);
            var csvContent = await streamReader.ReadToEndAsync();

            return new EasyCsv(csvContent, config);
        }

        internal async Task CalculateContentAsync()
        {
            await using var writer = new StringWriter();
            await using var csvWriter = new CsvWriter(writer, Config.CsvHelperConfig);
            var dynamicContent = CsvContent?.Cast<dynamic>();
            await csvWriter.WriteRecordsAsync(dynamicContent);
            var str = writer.ToString();
            ContentBytes = Encoding.UTF8.GetBytes(str);
            ContentStr = str;
        }

        internal void CalculateContent()
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

            await CalculateContentAsync();
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
                using var csvReader = new CsvReader(reader, EasyCsvConfiguration.Instance.CsvHelperConfig);
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

            await CalculateContentAsync();
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

            await CalculateContentAsync();
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
