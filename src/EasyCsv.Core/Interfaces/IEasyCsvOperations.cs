using System.Collections.Generic;
using System;
using CsvHelper.Configuration;
using CsvHelper;
using System.Threading.Tasks;

namespace EasyCsv.Core
{
    public interface IEasyCsvOperations
    {
        /// <summary>
        /// Replaces all the headers of the CSV.
        /// </summary>
        /// <example>You can turn header1,header2 into otherHeader1,otherHeader2</example>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv ReplaceHeaderRow(List<string> newHeaderFields);


        /// <summary>
        /// Removes the column of the old header field and upserts all it's values to all the rows of the new header field. CSV.
        /// </summary>
        /// <param name="oldHeaderField">The column that will be removed.</param>
        /// <param name="newHeaderField">The column that will contain all the values of the old column.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv ReplaceColumn(string oldHeaderField, string newHeaderField);


        /// <summary>
        /// Adds or replaces all the values for a given column in the CSV.
        /// </summary>
        /// <param name="header">The header field of the column you are giving a default value to.</param>
        /// <param name="value">The value you want every record in a column to have.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv AddColumn(string header, string value, bool upsert = true);


        /// <summary>
        /// Adds or replaces all the values for multiples columns in the CSV.
        /// </summary>
        /// <param name="defaultValues">Header Field, Default Value. Dictionary of the header fields of the columns you want to give a default value to.</param>
        /// /// <param name="upsert">Determines whether or not an exception is thrown if the column already exists.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv AddColumns(IDictionary<string, string> defaultValues, bool upsert = true);


        /// <summary>
        /// Removes rows from the CSV content that don't match the predicate.
        /// <param name="predicate">Predicate to filter rows</param>
        /// </summary>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv FilterRows(Func<CsvRow, bool> predicate);


        /// <summary>
        /// Replace values in a column based on the valueMapping dictionary
        /// </summary>
        /// <param name="headerField">The header field of the column to do value mapping.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping);


        /// <summary>
        /// Sorts the csv based on provided header field.
        /// </summary>
        /// <param name="headerField">The header field that you would like to use as the basis of the sorting.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv SortCsv(string headerField, bool ascending = true);


        /// <summary>
        /// Sorts the rows in the CSV content based on a custom key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the keySelector function.</typeparam>
        /// <param name="keySelector">A function that defines how to extract a key from each row for comparison.</param>
        /// <param name="ascending">A boolean value indicating whether the rows should be sorted in ascending order. If false, the rows will be sorted in descending order. The default value is true.</param>
        /// <example>
        /// This example sorts the rows by the length of the string in the "FieldName" column in descending order:
        /// <code>
        /// csvService.SortCsv(row => row["FieldName"].ToString().Length, ascending: false);
        /// </code>
        /// </example>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true);


        /// <summary>
        /// Removes a column from the CSV content.
        /// </summary>
        /// <param name="headerField">The header field of the column you want to remove.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv RemoveColumn(string headerField);


        /// <summary>
        /// Removes columns from the CSV content.
        /// </summary>
        /// <param name="headerFields">The header fields of the columns you want to remove.</param>
        /// <returns>An <code>IEasyCsv</code> to be used for fluent method chaining.</returns>
        IEasyCsv RemoveColumns(List<string> headerFields);


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
        /// Sets <code>CsvContent</code> to null. 
        /// </summary>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        IEasyCsv Clear();



        /// <summary>
        /// If all the headers match, adds all the data from other csv to this csv.
        /// </summary>
        /// <param name="otherCsv">The csv with the data you would like to be added to this one</param>
        /// <returns></returns>
        IEasyCsv Combine(IEasyCsv? otherCsv);


        /// <summary>
        /// Performs combine on multiple csvs. See <see cref="Combine(IEasyCsv?)"/>
        /// </summary>
        /// <param name="otherCsv">The csvs with the data you would like to be added to this one</param>
        /// <returns></returns>
        IEasyCsv Combine(List<IEasyCsv?>? otherCsv);
    }
}