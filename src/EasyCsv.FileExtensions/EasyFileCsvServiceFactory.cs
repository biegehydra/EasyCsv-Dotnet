using System;
using System.Threading.Tasks;
using EasyCsv.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace EasyCsv.Files
{
    public class EasyFileCsvServiceFactory : EasyCsvServiceFactory, IEasyFileCsvServiceFactory
    {
        public ICsvService? FromBrowserFile(IBrowserFile file, long maxFileSize, bool normalizeFields = false)
        {
            var csvService = new Core.EasyCsv(normalizeFields);
            if (!csvService.TryReadFileAsync(file, maxFileSize).GetAwaiter().GetResult()) return null;
            csvService.CreateCsvContent();
            return csvService.CsvContent == null ? null : csvService;
        }

        public async Task<ICsvService?> FromBrowserFileAsync(IBrowserFile file, long maxFileSize, bool normalizeFields = false)
        {
            var csvService = new Core.EasyCsv(normalizeFields);
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService.CsvContent == null ? null : csvService;
        }


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IFormFile</code> asynchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        /// /// <remarks>
        /// WARNING: This function is in case you don't want to or can't use dependency injection and is not recommended for use.
        /// </remarks>
        [Obsolete("Please use dependency injection instead unless absolutely not possible: FromBrowserFileAsync")]
        public static async Task<ICsvService?> StaticFromBrowserFile(IBrowserFile file, long maxFileSize, bool normalizeFields = false)
        {
            var csvService = new Core.EasyCsv(normalizeFields);
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService.CsvContent == null ? null : csvService;
        }

        public ICsvService? FromFormFile(IFormFile file, long maxFileSize, bool normalizeFields = false)
        {
            var csvService = new Core.EasyCsv(normalizeFields);
            if (!csvService.TryReadFileAsync(file, maxFileSize).GetAwaiter().GetResult()) return null;
            csvService.CreateCsvContent();
            return csvService?.CsvContent == null || csvService.CsvContent.Count < 0 ? null : csvService;

        }

        public async Task<ICsvService?> FromFormFileAsync(IFormFile file, long maxFileSize, bool normalizeFields = false)
        {
            var csvService = new Core.EasyCsv(normalizeFields);
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService;
        }

        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IFormFile</code> asynchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        /// /// <remarks>
        /// WARNING: This function is in case you don't want to or can't use dependency injection and is not recommended for use.
        /// </remarks>
        [Obsolete("Please use dependency injection instead unless absolutely not possible: FromFormFileAsync")]
        public static async Task<ICsvService?> StaticFromFormFile(IFormFile file, long maxFileSize, bool normalizeFields = false)
        {
            var csvService = new Core.EasyCsv(normalizeFields);
            if (!await csvService.TryReadFileAsync(file, maxFileSize)) return null;
            await csvService.CreateCsvContentInBackGround();
            return csvService;
        }

    }
}
