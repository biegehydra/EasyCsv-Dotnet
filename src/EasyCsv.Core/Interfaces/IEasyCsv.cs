using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace EasyCsv.Core
{
    public interface IEasyCsv : IEasyCsvBase<IEasyCsv>, IInternalCrud, IInternalOperations
    {
        Task CalculateContentBytesAndStrAsync();
        void CalculateContentBytesAndStr();

        /// <summary>
        /// Performs synchronous mutations on the CSV content using the provided delegate.
        /// </summary>
        /// <param name="mutations">A delegate that takes a <see cref="CSVMutationScope"/> and performs the desired mutations on the CSV content.</param>
        /// <param name="saveChanges">A boolean flag indicating whether to save the changes made by the mutations. If set to true, the method will update the internal CSV content representation. Default value is true.</param>
        /// <example>
        /// This example demonstrates how to use the Mutate method to add and remove a header.
        /// <code>
        /// <![CDATA[
        /// IEasyCsv csv = EasyCsvFactory.FromString("header1,header2\nvalue1,value2");
        /// csv.Mutate(scope =>
        /// {
        ///     scope.AddColumn("header3", "value3");
        ///     scope.RemoveColumn("header1");
        /// });
        /// // CSV content after mutation: "header2,header3\nvalue2,value3"
        /// ]]>
        /// </code>
        /// </example>
        void Mutate(Action<CSVMutationScope> mutations, bool saveChanges = true);


        /// <summary>
        /// Performs asynchronous mutations on the CSV content using the provided delegate.
        /// </summary>
        /// <param name="mutations">An async delegate that takes a <see cref="CSVMutationScope"/> and performs the desired mutations on the CSV content asynchronously.</param>
        /// <param name="saveChanges">A boolean flag indicating whether to save the changes made by the mutations. If set to true, the method will update the internal CSV content representation. Default value is true.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// This example demonstrates how to use the MutateAsync method to add and remove a header.
        /// <code>
        /// <![CDATA[
        /// IEasyCsv csv = EasyCsvFactory.FromString("header1,header2\nvalue1,value2");
        /// await csv.MutateAsync(async scope =>
        /// {
        ///     // Perform async operations here (if needed)
        ///     await Task.Delay(1000);
        ///     await easyCsv.RemoveUnusedHeadersAsync<Person>();
        ///
        ///     scope.AddColumn("header3", "value3");
        ///     scope.RemoveColumn("header1");
        /// });
        /// // CSV content after mutation: "header2,header3\nvalue2,value3"
        /// ]]>
        /// </code>
        /// </example>
        Task MutateAsync(Func<CSVMutationScope, Task> mutations, bool saveChanges = true);



        /// <summary>
        /// Calculates file content asynchronously but runs mutations synchronously.
        /// </summary>
        /// <param name="mutations">An async delegate that takes a <see cref="CSVMutationScope"/> and performs the desired mutations on the CSV content asynchronously.</param>
        /// <param name="saveChanges">A boolean flag indicating whether to save the changes made by the mutations. If set to true, the method will update the internal CSV content representation. Default value is true.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// This example demonstrates how to use the MutateAsync method to add and remove a header.
        /// <code>
        /// <![CDATA[
        /// IEasyCsv csv = new CsvManipulator("header1,header2\nvalue1,value2");
        /// await csv.MutateAsync(async scope =>
        /// {
        ///     scope.AddColumn("header3", "value3");
        ///     scope.RemoveColumn("header1");
        /// });
        /// // CSV content after mutation: "header2,header3\nvalue2,value3"
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>Useful when you are only doing synchronous mutations but want the benefits of calculating the content string and bytes asynchronously</remarks>
        Task MutateAsync(Action<CSVMutationScope> mutations, bool saveChanges = true);

        /// <summary>
        /// Gets row at index.
        /// </summary>
        /// <param name="index">Index of the row you want.</param>
        /// <returns>A <code>IDictionary string, object</code> representing properties and values of row. </returns>
        CsvRow? GetRow(int index);


        /// <summary>
        /// Gets row at index as object of type T.
        /// </summary>
        /// <param name="index">Index of the row you want.</param>
        /// <typeparam name="T">The type of object the row will be read into</typeparam>
        /// <returns>A <code>IDictionary string, object</code> representing properties and values of row. </returns>
        T? GetRow<T>(int index) where T : class;

        IEasyCsv CondenseTo(ICollection<string> columnNames, ICollection<int>? rowIndexes);
    }
}