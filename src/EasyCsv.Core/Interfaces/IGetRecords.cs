namespace EasyCsv.Core
{
    public interface IGetRecords
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
    }
}