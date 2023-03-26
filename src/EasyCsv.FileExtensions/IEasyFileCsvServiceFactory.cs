using System.Threading.Tasks;
using EasyCsv.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace EasyCsv.Files;

public interface IEasyFileCsvServiceFactory : ICsvServiceFactory, IEasyService
{
    ICsvService? CreateFromIBrowserFile(IBrowserFile file, long maxFileSize = 1024 * 1024 * 15);
    Task<ICsvService?> CreateFromIBrowserFileAsync(IBrowserFile file, long maxFileSize = 1024 * 1024 * 15);
    ICsvService? CreateFromIFormFile(IFormFile file, long maxFileSize = 1024 * 1024 * 15);
    Task<ICsvService?> CreateFromIFormFileAsync(IFormFile file, long maxFileSize = 1024 * 1024 * 15);

}