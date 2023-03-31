using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Components.Forms;
#endif
using Microsoft.AspNetCore.Http;

[assembly: InternalsVisibleTo("EasyCsv.Tests.Files")]
namespace EasyCsv.Files
{
    internal static class EasyCsvFileExtensions
    {
#if NET5_0_OR_GREATER
        internal static async Task<bool> TryReadFileAsync(this Core.EasyCsv easyCsv, IBrowserFile file, long maxFileSize)
        {
            try
            {
                if (file.Size > maxFileSize)
                {
                    throw new InvalidOperationException("File size too large");
                }
                await using (var memoryStream = new MemoryStream())
                {
                    await file.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
                    easyCsv.ContentBytes = memoryStream.ToArray();
                    easyCsv.ContentStr = Encoding.UTF8.GetString(easyCsv.ContentBytes);
                }
                await easyCsv.CreateCsvContentInBackGround();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading the file.", ex);
            }
        }

        internal static bool TryReadFile(this Core.EasyCsv easyCsv, IBrowserFile file, long maxFileSize)
        {
            try
            {
                if (file.Size > maxFileSize)
                {
                    throw new InvalidOperationException("File size too large");
                }
                using (var memoryStream = new MemoryStream())
                {
                    file.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
                    easyCsv.ContentBytes = memoryStream.ToArray();
                    easyCsv.ContentStr = Encoding.UTF8.GetString(easyCsv.ContentBytes);
                }
                easyCsv.CreateCsvContent();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading the file.", ex);
            }
        }
#endif
        internal static async Task<bool> TryReadFileAsync(this Core.EasyCsv easyCsv, IFormFile file, long maxFileSize)
        {
            try
            {
                if (file.Length > maxFileSize)
                {
                    //_logger.($"File size exceeds the maximum allowed size of {maxFileSize} bytes.");
                    return false;
                }
#if NETSTANDARD2_1_OR_GREATER
                await using var memoryStream = new MemoryStream();
#else
                using var memoryStream = new MemoryStream();
#endif
                await file.CopyToAsync(memoryStream);
                easyCsv.ContentBytes = memoryStream.ToArray();
                easyCsv.ContentStr = Encoding.UTF8.GetString(easyCsv.ContentBytes);
                await easyCsv.CreateCsvContentInBackGround();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading the file.", ex);
            }
        }

        internal static bool TryReadFile(this Core.EasyCsv easyCsv, IFormFile file, long maxFileSize)
        {
            try
            {
                if (file.Length > maxFileSize)
                {
                    //_logger.($"File size exceeds the maximum allowed size of {maxFileSize} bytes.");
                    return false;
                }

                using var memoryStream = new MemoryStream();
                file.CopyToAsync(memoryStream);
                easyCsv.ContentBytes = memoryStream.ToArray();
                easyCsv.ContentStr = Encoding.UTF8.GetString(easyCsv.ContentBytes);
                easyCsv.CreateCsvContent();
                return true;
            }
            catch (Exception ex)
            {
                 throw new InvalidOperationException("An error occurred while reading the file.", ex);
            }
        }
    }
}
