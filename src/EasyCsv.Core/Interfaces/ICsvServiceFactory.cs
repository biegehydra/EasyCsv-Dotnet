using System.IO;
using System.Threading.Tasks;

namespace EasyCsv.Core
{
    public interface ICsvServiceFactory : IEasyService
    {
        /// <summary>
        /// Creates <code>ICsvService</code> from <code>byte[]</code> synchronously
        /// </summary>
        /// <param name="fileContentBytes">Content of the CSV file</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns><code>ICsvService</code></returns>
        ICsvService? FromBytes(byte[] fileContentBytes, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>byte[]</code> as a background task
        /// </summary>
        /// <param name="fileContentBytes">Content of CSV</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns><code>ICsvService</code></returns>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        Task<ICsvService?> FromBytesAsync(byte[] fileContentBytes, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>string</code> synchronously
        /// </summary>
        /// <param name="fileContentStr">Content of CSV</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns><code>ICsvService</code></returns>
        ICsvService? FromString(string fileContentStr, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>string</code> as a background task
        /// </summary>
        /// <param name="fileContentStr">Content of CSV</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns><code>ICsvService</code></returns>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        Task<ICsvService?> FromStringAsync(string fileContentStr, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>Stream</code> synchronously
        /// </summary>
        /// <param name="fileStream">Stream of csv file</param>
        /// <param name="maxFileSize">Will throw exception if file is larger than file size</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns><code>ICsvService</code></returns>
        ICsvService? FromStream(Stream fileStream, int maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>Stream</code> asynchronously
        /// </summary>
        /// <param name="fileStream">Stream of csv file</param>
        /// <param name="maxFileSize">Will throw exception if file is larger than file size</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns><code>ICsvService</code></returns>
        Task<ICsvService?> FromStreamAsync(Stream fileStream, int maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>TextReader</code> synchronously
        /// </summary>
        /// <param name="fileContentReader">Stream of csv file</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        ICsvService? FromTextReader(TextReader fileContentReader, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>TextReader</code> as a background task
        /// </summary>
        /// <param name="fileContentReader">Stream of csv file</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        Task<ICsvService?> FromTextReaderAsync(TextReader fileContentReader, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>File</code> synchronously
        /// </summary>
        /// <param name="filePath">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        ICsvService? FromFile(string filePath, int maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>File</code> asynchronously
        /// </summary>
        /// <param name="filePath">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        Task<ICsvService?> FromFileAsync(string filePath, int maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>Url</code> synchronously
        /// </summary>
        /// <param name="url">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        ICsvService? FromUrl(string url, int maxFileSize = 1024 * 1024 * 15,  bool normalizeFields = false);


        /// <summary>
        /// Creates <code>ICsvService</code> from <code>Url</code> asynchronously
        /// </summary>
        /// <param name="url">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="normalizeFields">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        Task<ICsvService?> FromUrlAsync(string url, int maxFileSize = 1024 * 1024 * 15, bool normalizeFields = false);
    }

}
