using System;
using System.Threading.Tasks;
using EasyCsv.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace EasyCsv.Files;

public interface IEasyFileCsvServiceFactory : ICsvServiceFactory, IEasyService
{
    /// <summary>
    /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> synchronously
    /// </summary>
    /// <param name="file"></param>
    /// <param name="maxFileSize"></param>
    /// <returns><code>ICsvService</code></returns>
    [Obsolete("Please use the async version of this method unless absolutely not possible: CreateEasyCsvAsync")]
    ICsvService? FromBrowserFile(IBrowserFile file, long maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);
    /// <summary>
    /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> asynchronously
    /// </summary>
    /// <param name="file"></param>
    /// <param name="maxFileSize"></param>
    /// <returns><code>ICsvService</code></returns>
    Task<ICsvService?> FromBrowserFileAsync(IBrowserFile file, long maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);
    /// <summary>
    /// Creates <code>ICsvService</code> from <code>IBrowserFile</code> synchronously
    /// </summary>
    /// <param name="file"></param>
    /// <param name="maxFileSize"></param>
    /// <returns><code>ICsvService</code></returns>
    [Obsolete("Please use the async version of this method unless absolutely not possible: CreateEasyCsvAsync")]
    ICsvService? FromFormFile(IFormFile file, long maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);
    /// <summary>
    /// Creates <code>ICsvService</code> from <code>IFormFile</code> asynchronously
    /// </summary>
    /// <param name="file"></param>
    /// <param name="maxFileSize"></param>
    /// <returns><code>ICsvService</code></returns>
    Task<ICsvService?> FromFormFileAsync(IFormFile file, long maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);

}