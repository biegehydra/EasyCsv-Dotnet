using System.Threading.Tasks;
using EasyCsv.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace EasyCsv.Files
{
    internal class EasyFileCsvServiceFactory : EasyCsvServiceFactory, IEasyFileCsvServiceFactory
    {
        public ICsvService? CreateFromIBrowserFile(IBrowserFile file, long maxFileSize)
        {
            var csvService = new Core.EasyCsv();
            if (!csvService.TryReadFileAsync(file, maxFileSize).GetAwaiter().GetResult()) return null;
            csvService.CreateCsvContent();
            return csvService.CsvContent == null ? null : csvService;
        }

        public async Task<ICsvService?> CreateFromIBrowserFileAsync(IBrowserFile file, long maxFileSize)
        {
            var csvService = new Core.EasyCsv();
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService.CsvContent == null ? null : csvService;
        }


        public static async Task<ICsvService?> StaticCreateFromIBrowserFileAsync(IBrowserFile file, long maxFileSize)
        {
            var csvService = new Core.EasyCsv();
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService.CsvContent == null ? null : csvService;
        }

        public ICsvService? CreateFromIFormFile(IFormFile file, long maxFileSize)
        {
            var csvService = new Core.EasyCsv();
            if (!csvService.TryReadFileAsync(file, maxFileSize).GetAwaiter().GetResult()) return null;
            csvService.CreateCsvContent();
            return csvService?.CsvContent == null || csvService.CsvContent.Count < 0 ? null : csvService;

        }

        public async Task<ICsvService?> CreateFromIFormFileAsync(IFormFile file, long maxFileSize)
        {
            var csvService = new Core.EasyCsv();
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService;
        }

        public static async Task<ICsvService?> StaticCreateFromIFormFileAsync(IFormFile file, long maxFileSize)
        {
            var csvService = new Core.EasyCsv();
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService;
        }

    }
}
