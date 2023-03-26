using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("EasyCsv")]
namespace EasyCsv.Core
{
    internal class EasyCsvServiceFactory : ICsvServiceFactory
    {
        public ICsvService? CreateFromBytes(byte[] fileContent, bool normalizeFields = false)
        {
            var csvService = new EasyCsv(fileContent, normalizeFields);
            return csvService.CsvContent == null || csvService.CsvContent.Count < 0 ? null : csvService;
        }

        public async Task<ICsvService?> CreateFromBytesInBackground(byte[] fileContent, bool normalizeFields = false)
        {
            return await CreateCsvServiceInBackground(fileContent, Encoding.UTF8.GetString(fileContent), normalizeFields);
        }

        public ICsvService? CreateFromString(string fileContent, bool normalizeFields = false)
        {
            var csvService = new EasyCsv(fileContent, normalizeFields);
            return csvService.CsvContent == null || csvService.CsvContent.Count < 0 ? null : csvService;
        }

        public async Task<ICsvService?> CreateFromStringInBackground(string fileContent, bool normalizeFields = false)
        {
            return await CreateCsvServiceInBackground(Encoding.UTF8.GetBytes(fileContent), fileContent, normalizeFields);
        }

        private async Task<ICsvService?> CreateCsvServiceInBackground(byte[] fileContentByte, string fileContentStr, bool normalizeFields)
        {
            var service = new EasyCsv()
            {
                FileContentBytes = fileContentByte,
                FileContentStr = fileContentStr,
                NormalizeFields = normalizeFields
            };
            await service.CreateCsvContentInBackGround();
            return service;
        }
    }
}
