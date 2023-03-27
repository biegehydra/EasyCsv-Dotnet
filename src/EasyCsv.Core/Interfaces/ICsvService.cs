using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace EasyCsv.Core
{
    public interface ICsvService
    {
        byte[]? FileContentBytes { get; set; }
        string? FileContentStr { get; set; }
        List<IDictionary<string, object>>? CsvContent { get; }


        /// <summary>
        /// Gets the headers of the CSV file.
        /// </summary>
        /// <returns>An IEnumerable of string containing the CSV headers.</returns>
        List<string>? GetHeaders();


        /// <summary>
        /// Replaces all the headers of the CSV.
        /// </summary>
        /// <example>You can turn header1,header2 into otherHeader1,otherHeader2</example>
        /// <returns>An IEnumerable of string containing the CSV headers.</returns>
        ICsvService ReplaceHeaderRow(List<string> newHeaderFields);


        /// <summary>
        /// Removes the column of the old header field and upserts all it's values to all the rows of the new header field. CSV.
        /// </summary>
        /// <param name="oldHeaderField">The column that will be removed.</param>
        /// <param name="newHeaderField">The column that will contain all the values of the old column.</param>
        ICsvService ReplaceColumn(string oldHeaderField, string newHeaderField);


        /// <summary>
        /// Adds or replaces all the values for a given column in the CSV.
        /// </summary>
        /// <param name="header">The header field of the column you are giving a default value to.</param>
        /// <param name="value">The value you want every record in a column to have.</param>
        ICsvService AddColumn(string header, string value, bool upsert = true);


        /// <summary>
        /// Adds or replaces all the values for multiples columns in the CSV.
        /// </summary>
        /// <param name="defaultValues">Header Field, Default Value. Dictionary of the header fields of the columns you want to give a default value to.</param>
        /// /// <param name="upsert">Determines whether or not an exception is thrown if the column already exists.</param>
        ICsvService AddColumns(Dictionary<string, string> defaultValues, bool upsert = true);


        /// <summary>
        /// Removes rows from the CSV content that don't match the predicate.
        /// <param name="predicate">Predicate to filter rows</param>
        /// </summary>
        ICsvService FilterRows(Func<IDictionary<string, object>, bool> predicate);


        /// <summary>
        /// Replace values in a column based on the valueMapping dictionary
        /// </summary>
        /// <param name="headerField">The header field of the column to do value mapping.</param>
        ICsvService MapValuesInColumn(string headerField, Dictionary<object, object> valueMapping);


        /// <summary>
        /// Sorts the csv based on provided header field.
        /// </summary>
        /// <param name="headerField">The header field that you would like to use as the basis of the sorting.</param>
        ICsvService SortCsv(string headerField, bool ascending = true);


        /// <summary>
        /// Sorts the rows in the CSV content based on a custom key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the keySelector function.</typeparam>
        /// <param name="keySelector">A function that defines how to extract a key from each row for comparison.</param>
        /// <param name="ascending">A boolean value indicating whether the rows should be sorted in ascending order. If false, the rows will be sorted in descending order. The default value is true.</param>
        /// <example>
        /// This example sorts the rows by the length of the string in the "FieldName" column in descending order:
        /// <code>
        /// csvService.SortRows(row => row["FieldName"].ToString().Length, ascending: false);
        /// </code>
        /// </example>
        ICsvService SortRows<TKey>(Func<IDictionary<string, object>, TKey> keySelector, bool ascending = true);


        /// <summary>
        /// Removes columns from the CSV content.
        /// </summary>
        /// <param name="headerFields">The header fields of the columns you want to remove.</param>
        ICsvService RemoveColumns(List<string> headerFields);


        /// <summary>
        /// Removes a column from the CSV content.
        /// </summary>
        /// <param name="headerField">The header field of the column you want to remove.</param>
        ICsvService RemoveColumn(string headerField);


        /// <summary>
        /// Calculates the <code>FileContentStr</code> amd <code>FileContentBytes</code>.
        /// </summary>
        Task CalculateFileContent();

        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="strict">Determine whether property matching is case sensitive</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<T>> GetRecords<T>(bool strict = false);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="prepareHeaderForMatch">Determine whether property matching is case sensitive</param>
        /// <example><code>
        /// PrepareHeaderForMatch = args => args.Header.ToLower();
        /// </code></example>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<T>> GetRecords<T>(PrepareHeaderForMatch prepareHeaderForMatch);


        /// <summary>
        /// Gets the records of the CSV content as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="csvConfig">The configuration for reading the csv into records</param>
        /// <returns>A list of objects of type T representing the CSV records.</returns>
        Task<List<T>> GetRecords<T>(CsvConfiguration csvConfig);
    }
}