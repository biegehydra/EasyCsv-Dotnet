using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace EasyCsv.Files
{
    public static class EasyCsvFileFactory
    {
        private static IEasyCsv? NullOrEasyCsv(IEasyCsv? easyCsv) => easyCsv?.CsvContent == null || easyCsv.CsvContent.Count < 0 ? null : easyCsv;
        private static EasyCsvConfiguration GlobalConfig => EasyCsvConfiguration.Instance;
        private static EasyCsvConfiguration UserConfigOrGlobalConfig(EasyCsvConfiguration? userConfig) => userConfig ?? GlobalConfig; 

        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> synchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static IEasyCsv? FromBrowserFile(IBrowserFile file, long maxFileSize, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(UserConfigOrGlobalConfig(config));
            if (!easyCsv.TryReadFile(file, maxFileSize)) return null;
            easyCsv.CreateCsvContent();
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> asynchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static async Task<IEasyCsv?> FromBrowserFileAsync(IBrowserFile file, long maxFileSize, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(UserConfigOrGlobalConfig(config));
            if (!await easyCsv.TryReadFileAsync(file, maxFileSize)) return null;
            await easyCsv.CreateCsvContentInBackGround();
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> synchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static IEasyCsv? FromFormFile(IFormFile file, long maxFileSize, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(UserConfigOrGlobalConfig(config));
            if (!easyCsv.TryReadFile(file, maxFileSize)) return null;
            easyCsv.CreateCsvContent();
            return NullOrEasyCsv(easyCsv);

        }


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IFormFile</code> asynchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static async Task<IEasyCsv?> FromFormFileAsync(IFormFile file, long maxFileSize, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(UserConfigOrGlobalConfig(config));
            if (!await easyCsv.TryReadFileAsync(file, maxFileSize)) return null;
            await easyCsv.CreateCsvContentInBackGround();
            return NullOrEasyCsv(easyCsv);
        }
    }
}
