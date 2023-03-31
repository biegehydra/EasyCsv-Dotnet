using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace EasyCsv.Core
{
    public partial interface IEasyCsv : ICloneable
    {
        /// <summary>
        /// Content of the csv file in bytes. Calculate on creation and when <code>CalculateContentAsync</code> is called
        /// </summary>
        byte[]? ContentBytes { get; }


        /// <summary>
        /// Content of the csv file in as a string. Calculated on creation and when <code>CalculateContentAsync</code> is called
        /// </summary>
        string? ContentStr { get; }


        /// <summary>
        /// The content of the CSV file as a list of dictionaries representing kvps of the headers and values.
        /// </summary>
        List<IDictionary<string, object>>? Content { get; }


        /// <summary>
        /// Gets the number of rows in the CSV file.
        /// <returns>Number of rows in csv</returns>
        /// </summary>
        int GetRowCount();


        /// <summary>
        /// Returns true if csv container header field with value provided
        /// </summary>
        bool ContainsHeader(string headerField, bool caseInsensitive);


        /// <summary>
        /// Determines whether the CSV contains the specified row.
        /// </summary>
        /// <param name="row">The row to check for, represented as an IDictionary of field names and their corresponding values.</param>
        /// <returns>True if the CSV contains the specified row; otherwise, False.</returns>
        bool ContainsRow(IDictionary<string, object> row);


        /// <summary>
        /// Gets the headers of the CSV file.
        /// </summary>
        /// <returns>An IEnumerable of string containing the CSV headers.</returns>
        List<string>? GetHeaders();


        /// <summary>
        /// Calculates the <code>ContentStr</code> amd <code>ContentBytes</code> asynchronously.
        /// </summary>
        Task CalculateContentAsync();


        /// <summary>
        /// Calculates the <code>ContentStr</code> amd <code>ContentBytes</code> synchronously.
        /// </summary>
        void CalculateContent();


        /// <summary>
        /// Removes any header that does match a public property on the type param T
        /// <typeparam name="T">The type that will be used as the basis for what headers to remove</typeparam>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        /// <param name="caseInsensitive">Determines whether the operation should be case insensitive when determining if a header field and all the values in it's column should be removed--are UNUSED.</param>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        /// </summary>
        Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(bool caseInsensitive = true);


        /// <summary>
        /// Removes any header that does match a public property on the type param T
        /// <typeparam name="T">The type that will be used as the basis for what headers to remove</typeparam>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        /// <param name="prepareHeaderForMatch">Determines whether the operation should be case insensitive when determining if a header field, and all the values in it's column, should be removed--are UNUSED. See GetGetRecordsAsync for information on prepareHeaderForMatch usage.</param>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        /// </summary>
        Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch);


        /// <summary>
        /// Removes any header that does match a public property on the type param T
        /// <typeparam name="T">The type that will be used as the basis for what headers to remove</typeparam>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        /// <param name="csvConfig">The CSVHelper csv configuration configuration for reading the csv into records, which ultimately removes unused headers</param>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        /// </summary>
        Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="caseInsensitive">Determine whether property matching is case sensitive</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<T>> GetRecordsAsync<T>(bool caseInsensitive = false);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="prepareHeaderForMatch">Determine whether property matching is case sensitive</param>
        /// <example><code>
        /// PrepareHeaderForMatch = args => args.Header.ToLower();
        /// </code></example>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="csvConfig">The CSVHelper csv configuration configuration for reading the csv into records</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig);

        /// <summary>
        /// Create deep clone
        /// <returns>Deep clone of current <code>IEasyCsv</code></returns>
        /// </summary>
        IEasyCsv Clone();
    }
}