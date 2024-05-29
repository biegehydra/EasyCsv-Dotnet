using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    public interface IEasyCsvBase<T>
    {
        /// <summary>
        /// CsvContent of the csv file in bytes. Calculate on creation and when <code>CalculateContentBytesAndStrAsync</code> is called
        /// </summary>
        byte[]? ContentBytes { get; }


        /// <summary>
        /// CsvContent of the csv file in as a string. Calculated on creation and when <code>CalculateContentBytesAndStrAsync</code> is called
        /// </summary>
        string? ContentStr { get; }


        /// <summary>
        /// The content of the CSV file as a list of dictionaries representing kvps of the headers and values.
        /// </summary>
        List<CsvRow> CsvContent { get; }

        internal void SetCsvContent(List<CsvRow> newRows);

        /// <summary>
        /// Gets the number of rows in the CSV file.
        /// <returns>Number of rows in csv</returns>
        /// </summary>
        int RowCount();


        /// <summary>
        /// Returns true if csv container header field with value provided
        /// </summary>
        bool ContainsColumn(string column, bool caseInsensitive = false);


        bool ContainsAllColumns(IEnumerable<string> columns, IEqualityComparer<string>? comparer = null);


        /// <summary>
        /// Determines whether the CSV contains the specified row.
        /// </summary>
        /// <param name="row">The row to check for, represented as an IDictionary of field names and their corresponding values.</param>
        /// <returns>True if the CSV contains the specified row; otherwise, False.</returns>
        bool ContainsRow(CsvRow row);


        /// <summary>
        /// Gets the headers of the CSV file.
        /// </summary>
        /// <returns>An IEnumerable of string containing the CSV headers.</returns>
        string[]? ColumnNames();


        int ColumnIndex(string columnName);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="TRecord">The type of objects to return.</typeparam>
        /// <param name="strict">Determine whether property matching is case sensitive</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<TRecord>> GetRecordsAsync<TRecord>(bool strict = false, CsvContextProfile? csvContextProfile = null);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="TRecord">The type of objects to return.</typeparam>
        /// <param name="prepareHeaderForMatch">Determine whether property matching is case sensitive</param>
        /// <example><code>
        /// PrepareHeaderForMatch = args => args.Header.ToLower();
        /// </code></example>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<TRecord>> GetRecordsAsync<TRecord>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="TRecord">The type of objects to return.</typeparam>
        /// <param name="csvConfig">The CSVHelper csv configuration configuration for reading the csv into records</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<TRecord>> GetRecordsAsync<TRecord>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="TRecord">The type of objects to return.</typeparam>
        /// <param name="csvConfig">The CSVHelper csv configuration configuration for reading the csv into records</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        IAsyncEnumerable<TRecord> ReadRecordsAsync<TRecord>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null);

        /// <summary>
        /// If all the headers match, adds all the data from other csv to this csv.
        /// </summary>
        /// <param name="otherCsv">The csv with the data you would like to be added to this one</param>
        /// <returns></returns>
        T Combine(IEasyCsv? otherCsv);


        /// <summary>
        /// Performs combine on multiple csvs. See <see cref="Combine(IEasyCsv?)"/>
        /// </summary>
        /// <param name="otherCsv">The csvs with the data you would like to be added to this one</param>
        /// <returns></returns>
        T Combine(List<IEasyCsv?> otherCsv);


        /// <summary>
        /// Create deep clone
        /// <returns>Deep clone of current IEasyCsv</returns>
        /// </summary>
        T Clone();

        /// <summary>
        /// Sets <code>CsvContent</code> to null. 
        /// </summary>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        T Clear();
    }
}