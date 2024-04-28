using System.Collections.Generic;
using System;
using CsvHelper.Configuration;
using CsvHelper;
using System.Threading.Tasks;
using EasyCsv.Core.Configuration;
using EasyCsv.Core.Enums;

namespace EasyCsv.Core
{
    public interface IEasyCsvOperations<T>
    {
        /// <summary>
        /// Replaces all the headers of the CSV.
        /// </summary>
        /// <example>You can turn header1,header2 into otherHeader1,otherHeader2</example>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T ReplaceHeaderRow(List<string> newHeaderFields);


        /// <summary>
        /// Removes the column of the old header field and upserts all it's values to all the rows of the new header field. CSV.
        /// </summary>
        /// <param name="oldHeaderField">The column that will be removed.</param>
        /// <param name="newHeaderField">The column that will contain all the values of the old column.</param>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T ReplaceColumn(string oldHeaderField, string newHeaderField);


        /// <summary>
        /// Adds or replaces all the values for a given column in the CSV.
        /// </summary>
        /// <param name="header">The header field of the column you are giving a default value to.</param>
        /// <param name="value">The value you want every record in a column to have.</param>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T AddColumn(string header, object? value, ExistingColumnHandling existingColumnHandling = ExistingColumnHandling.Override);

        T MoveColumn(int oldIndex, int newIndex);
        T MoveColumn(string columnName, int newIndex);

        T InsertColumn(int index, string columnName, object? defaultValue);

        /// <summary>
        /// Adds or replaces all the values for multiples columns in the CSV.
        /// </summary>
        /// <param name="defaultValues">Header Field, Default Value. Dictionary of the header fields of the columns you want to give a default value to.</param>
        /// <param name="upsert">Determines whether or not an exception is thrown if the column already exists.</param>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T AddColumns(IDictionary<string, object?> defaultValues, ExistingColumnHandling existingColumnHandling = ExistingColumnHandling.Override);


        /// <summary>
        /// Removes rows from the CSV content that don't match the predicate.
        /// <param name="predicate">Predicate to filter rows</param>
        /// </summary>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T FilterRows(Func<CsvRow, bool> predicate);


        /// <summary>
        /// Replace values in a column based on the valueMapping dictionary
        /// </summary>
        /// <param name="headerField">The header field of the column to do value mapping.</param>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping);


        /// <summary>
        /// Sorts the csv based on provided header field.
        /// </summary>
        /// <param name="headerField">The header field that you would like to use as the basis of the sorting.</param>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T SortCsv(string headerField, bool ascending = true);


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
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true);


        /// <summary>
        /// Removes a column from the CSV content.
        /// </summary>
        /// <param name="headerField">The header field of the column you want to remove.</param>
        /// <param name="throwIfNotExists">Determines whether the function will throw and exception or do nothing if the specified header field doesn't exist.</param>
        /// <exception cref="ExceptionType">Description of the conditions under which the exception is thrown.</exception>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T RemoveColumn(string column, bool throwIfNotExists = true);


        /// <summary>
        /// Removes columns from the CSV content.
        /// </summary>
        /// <param name="headerFields">The header fields of the columns you want to remove.</param>
        /// <param name="throwIfNotExists">Determines whether the function will throw and exception or do nothing if the specified header field doesn't exist</param>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        T RemoveColumns(IEnumerable<string> columns, bool throwIfNotExists = true);

        /// <summary>
        /// Removes all the columns where the value of the column is null or whitespace in all rows.
        /// </summary>
        /// <returns></returns>
        T RemoveUnusedHeaders();

        /// <summary>
        /// Removes any header that does match a public property on the type param T
        /// <typeparam name="TClass">The type that will be used as the basis for what headers to remove</typeparam>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        /// <param name="strict">Determines whether the operation should be lenient in getting values and matching headers.</param>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        /// </summary>
        Task<T> RemoveUnusedHeadersAsync<TClass>(bool strict = true, CsvContextProfile? csvContextProfile = null);


        /// <summary>
        /// Removes any header that does match a public property on the type param T
        /// <typeparam name="TClass">The type that will be used as the basis for what headers to remove</typeparam>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        /// <param name="prepareHeaderForMatch">Determines whether the operation should be case insensitive when determining if a header field, and all the values in it's column, should be removed--are UNUSED. See GetGetRecordsAsync for information on prepareHeaderForMatch usage.</param>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        /// </summary>
        Task<T> RemoveUnusedHeadersAsync<TClass>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null);


        /// <summary>
        /// Removes any header that does match a public property on the type param T
        /// <typeparam name="TClass">The type that will be used as the basis for what headers to remove</typeparam>
        /// <returns>An IEasyCsv to be used for fluent method chaining.</returns>
        /// <param name="csvConfig">The CSVHelper csv configuration configuration for reading the csv into records, which ultimately removes unused headers</param>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        /// </summary>
        Task<T> RemoveUnusedHeadersAsync<TClass>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null);

        T SwapColumns(int columnOneIndex, int columnTwoIndex);

        T SwapColumns(string columnOneName, string columnTwoName, IEqualityComparer<string>? comparer = null);
    }
}