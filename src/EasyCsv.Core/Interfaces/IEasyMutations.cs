using System.Collections.Generic;

namespace EasyCsv.Core
{
    public interface IEasyMutations : IEasyCsvBase<IEasyMutations>, IEasyCsvOperations<IEasyMutations>
    {
        /// <summary>
        /// Gets row at index.
        /// </summary>
        /// <param name="index">Index of the row you want.</param>
        /// <returns>A <code>IDictionary string, object</code> representing properties and values of row. </returns>
        CsvRow? GetRecord(int index);


        /// <summary>
        /// Gets row at index as object of type T.
        /// </summary>
        /// <param name="index">Index of the row you want.</param>
        /// <typeparam name="T">The type of object the row will be read into</typeparam>
        /// <returns>A <code>IDictionary string, object</code> representing properties and values of row. </returns>
        T? GetRecord<T>(int index) where T : class;

        #region CRUD


        /// <summary>
        /// Inserts a new row into the CSV at the specified index. If the index is -1, the row will be appended to the end of the CSV.
        /// </summary>
        /// <param name="rowValues">The row to insert, represented as an list of strings representing the values of the row.</param>
        /// <param name="index">The index at which to insert the row. If the index is -1, the row will be appended to the end of the CSV.</param>
        /// <returns>An IEasyCsv instance with the row inserted.</returns>
        IEasyMutations InsertRecord(List<object?> rowValues, int index = -1);


        /// <summary>
        /// Updates an existing row in the CSV or inserts a new row if the specified row doesn't already exist.
        /// </summary>
        /// <param name="row">The row to update or insert, represented as an IDictionary of field names and their corresponding values.</param>
        /// <returns>An IEasyCsv instance with the row updated or inserted.</returns>
        IEasyMutations UpsertRecord(CsvRow row, int index = -1);


        /// <summary>
        /// Updates existing rows in the CSV or inserts new rows if the specified rows don't already exist.
        /// </summary>
        /// <param name="rows">An IEnumerable of rows to update or insert, each represented as an IDictionary of field names and their corresponding values.</param>
        /// <returns>An IEasyCsv instance with the rows updated or inserted.</returns>
        IEasyMutations UpsertRecords(IEnumerable<CsvRow> rows);



        /// <summary>
        /// Update row at index.
        /// </summary>
        /// <param name="index">Index of row to update</param>
        /// <param name="newRow">The row to update or insert, represented as an IDictionary of field names and their corresponding values.</param>
        /// <remarks>Very prone to break. Not recommended to use right now. Better implementation will be added that will probably create breaking change</remarks>
        /// <returns></returns>
        IEasyMutations UpdateRecord(int index, CsvRow newRow);



        /// <summary>
        /// Delete row at index.
        /// </summary>
        /// <param name="index">Index of row to delete</param>
        /// <returns></returns>
        IEasyMutations DeleteRecord(int index);

        #endregion
    }
}