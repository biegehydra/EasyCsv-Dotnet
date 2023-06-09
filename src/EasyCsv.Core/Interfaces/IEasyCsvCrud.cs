﻿using System.Collections.Generic;

namespace EasyCsv.Core
{
    public interface IEasyCsvCrud<T>
    {
        /// <summary>
        /// Inserts a new row into the CSV at the specified index. If the index is -1, the row will be appended to the end of the CSV.
        /// </summary>
        /// <param name="rowValues">The row to insert, represented as an list of strings representing the values of the row.</param>
        /// <param name="index">The index at which to insert the row. If the index is -1, the row will be appended to the end of the CSV.</param>
        /// <returns>An IEasyCsv instance with the row inserted.</returns>
        T AddRecord(List<string> rowValues, int index = -1);


        /// <summary>
        /// Inserts a new row into the CSV at the specified index. If the index is -1, the row will be appended to the end of the CSV.
        /// </summary>
        /// <param name="rowValues">The row to insert, represented as an list of strings representing the values of the row.</param>
        /// <param name="index">The index at which to insert the row. If the index is -1, the row will be appended to the end of the CSV.</param>
        /// <returns>An IEasyCsv instance with the row inserted.</returns>
        T InsertRecord(List<string> rowValues, int index = -1);


        /// <summary>
        /// Updates an existing row in the CSV or inserts a new row if the specified row doesn't already exist.
        /// </summary>
        /// <param name="row">The row to update or insert, represented as an IDictionary of field names and their corresponding values.</param>
        /// <returns>An IEasyCsv instance with the row updated or inserted.</returns>
        T UpsertRecord(CsvRow row, int index = -1);


        /// <summary>
        /// Updates existing rows in the CSV or inserts new rows if the specified rows don't already exist.
        /// </summary>
        /// <param name="rows">An IEnumerable of rows to update or insert, each represented as an IDictionary of field names and their corresponding values.</param>
        /// <returns>An IEasyCsv instance with the rows updated or inserted.</returns>
        T UpsertRecords(IEnumerable<CsvRow> rows);



        /// <summary>
        /// Update row at index.
        /// </summary>
        /// <param name="index">Index of row to update</param>
        /// <param name="newRow">The row to update or insert, represented as an IDictionary of field names and their corresponding values.</param>
        /// <remarks>Very prone to break. Not recommended to use right now. Better implementation will be added that will probably create breaking change</remarks>
        /// <returns></returns>
        T UpdateRecord(int index, CsvRow newRow);



        /// <summary>
        /// Delete row at index.
        /// </summary>
        /// <param name="index">Index of row to delete</param>
        /// <returns></returns>
        T DeleteRecord(int index);

    }
}