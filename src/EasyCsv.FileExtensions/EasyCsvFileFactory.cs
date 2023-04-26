using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Components.Forms;
#endif
using Microsoft.AspNetCore.Http;

namespace EasyCsv.Files
{
    public static class EasyCsvFileFactory
    {
        private static IEasyCsv? NullOrEasyCsv(IEasyCsv? easyCsv) => easyCsv?.CsvContent == null || easyCsv.CsvContent.Count < 0 ? null : easyCsv;
        private static EasyCsvConfiguration GlobalConfig => EasyCsvConfiguration.Instance;
        private static EasyCsvConfiguration UserConfigOrGlobalConfig(EasyCsvConfiguration? userConfig) => userConfig ?? GlobalConfig;

#if NET5_0_OR_GREATER
        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> synchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static IEasyCsv? FromBrowserFile(IBrowserFile file, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(file.OpenReadStream(), UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }

        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> asynchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static async Task<IEasyCsv?> FromBrowserFileAsync(IBrowserFile file, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(UserConfigOrGlobalConfig(config));
            await easyCsv.CreateCsvContentInBackGround(file.OpenReadStream());
            await easyCsv.CalculateContentBytesAndStrAsync();
            return NullOrEasyCsv(easyCsv);
        }
#endif

        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> synchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static IEasyCsv? FromFormFile(IFormFile file, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(file.OpenReadStream(), UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);

        }


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>IFormFile</code> asynchronously
        /// </summary>
        /// <param name="file"></param>
        /// <param name="maxFileSize"></param>
        /// <returns><code>ICsvService</code></returns>
        public static async Task<IEasyCsv?> FromFormFileAsync(IFormFile file, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new Core.EasyCsv(UserConfigOrGlobalConfig(config));
            await easyCsv.CreateCsvContentInBackGround(file.OpenReadStream());
            await easyCsv.CalculateContentBytesAndStrAsync();
            return NullOrEasyCsv(easyCsv);
        }
    }
}
