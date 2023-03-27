using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("EasyCsv")]
namespace EasyCsv.Core
{
    public class EasyCsvServiceFactory : ICsvServiceFactory
    {
        private static ICsvService? NullOrEasyCsv(ICsvService? easyCsv) => easyCsv.CsvContent == null || easyCsv.CsvContent.Count < 0 ? null : easyCsv;

        public ICsvService? FromBytes(byte[] fileContentBytes, bool normalizeFields = false)
        {
            var easyCsv = new EasyCsv(fileContentBytes, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        public async Task<ICsvService?> FromBytesAsync(byte[] fileContentBytes, bool normalizeFields = false)
        {
            return await CreateCsvServiceInBackground(fileContentBytes, Encoding.UTF8.GetString(fileContentBytes), normalizeFields);
        }

        public ICsvService? FromString(string fileContentStr, bool normalizeFields = false)
        {
            var easyCsv = new EasyCsv(fileContentStr, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        public async Task<ICsvService?> FromStringAsync(string fileContentStr, bool normalizeFields = false)
        {
            return await CreateCsvServiceInBackground(Encoding.UTF8.GetBytes(fileContentStr), fileContentStr, normalizeFields);
        }

        private async Task<ICsvService?> CreateCsvServiceInBackground(byte[] fileContentByte, string fileContentStr, bool normalizeFields)
        {
            var easyCsv = new EasyCsv(normalizeFields)
            {
                FileContentBytes = fileContentByte,
                FileContentStr = fileContentStr,
            };
            await easyCsv.CreateCsvContentInBackGround();
            return NullOrEasyCsv(easyCsv);
        }

        public ICsvService? FromStream(Stream fileStream, int maxFileSize, bool normalizeFields = false)
        {
            var easyCsv = new EasyCsv(fileStream, maxFileSize, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        public async Task<ICsvService?> FromStreamAsync(Stream fileStream, int maxFileSize, bool normalizeFields = false)
        {
            var fileContent = await ReadStreamToEndAsync(fileStream, maxFileSize);
            var easyCsv = new EasyCsv(fileContent, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        private static async Task<byte[]> ReadStreamToEndAsync(Stream stream, int maxFileSize)
        {
            using var memoryStream = new MemoryStream(maxFileSize);
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public ICsvService? FromTextReader(TextReader fileContentReader, bool normalizeFields = false)
        {
            var easyCsv = new EasyCsv(fileContentReader, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        public async Task<ICsvService?> FromTextReaderAsync(TextReader fileContentReader, bool normalizeFields = false)
        {
            var fileContentStr = await fileContentReader.ReadToEndAsync();
            var easyCsv = await CreateCsvServiceInBackground(Encoding.UTF8.GetBytes(fileContentStr), fileContentStr, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        public ICsvService? FromFile(string filePath, int maxFileSize, bool normalizeFields = false)
        {
            var bytes = ReadFromFile(filePath, maxFileSize);
            var easyCsv = new EasyCsv(bytes, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        private static byte[] ReadFromFile(string filePath, int maxFileSize)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo == null || fileInfo.Length > maxFileSize)
            {
                throw new InvalidOperationException("File size too large");
            }
            return File.ReadAllBytes(filePath);
        }

        public async Task<ICsvService?> FromFileAsync(string filePath, int maxFileSize, bool normalizeFields = false)
        {
            var bytes = await ReadFromFileAsync(filePath, maxFileSize);
            var easyCsv = await FromBytesAsync(bytes, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        private static async Task<byte[]> ReadFromFileAsync(string filePath, int maxFileSize)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo == null || fileInfo.Length > maxFileSize)
            {
                throw new InvalidOperationException("File size too large");
            }
            return await File.ReadAllBytesAsync(filePath);
        }

        public ICsvService? FromUrl(string url, int maxFileSize, bool normalizeFields = false)
        {
            using var httpClient = new HttpClient();
            using var responseStream = httpClient.GetStreamAsync(url).GetAwaiter().GetResult();
            var easyCsv = new EasyCsv(responseStream, maxFileSize, normalizeFields);
            return NullOrEasyCsv(easyCsv);
        }

        public async Task<ICsvService?> FromUrlAsync(string url, int maxFileSize, bool normalizeFields = false)
        {
            using var httpClient = new HttpClient();
            using var responseStream = await httpClient.GetStreamAsync(url);
            return await FromStreamAsync(responseStream, maxFileSize, normalizeFields);
        }
    }
}
