using System.Threading.Tasks;
using System;

namespace EasyCsv.Core
{
    public interface IEasyCsv : IEasyCsvBase<IEasyCsv>, IInternalCrud, IInternalOperations, IGetRecords
    {
        /// <summary>
        /// Performs synchronous mutations on the CSV content using the provided delegate.
        /// </summary>
        /// <param name="mutations">A delegate that takes a <see cref="CSVMutationScope"/> and performs the desired mutations on the CSV content.</param>
        /// <param name="saveChanges">A boolean flag indicating whether to save the changes made by the mutations. If set to true, the method will update the internal CSV content representation. Default value is true.</param>
        /// <param name="safe">Determines whether to operate on a clone. Only useful is you don't save change</param>
        /// <example>
        /// This example demonstrates how to use the Mutate method to add and remove a header.
        /// <code>
        /// <![CDATA[
        /// var csvManipulator = new CsvManipulator("header1,header2\nvalue1,value2");
        /// csvManipulator.Mutate(scope =>
        /// {
        ///     // CSV content before mutation: "header1,header2\nvalue1,value2"
        ///     scope.AddColumn("header3", "value3");
        ///     scope.RemoveColumn("header1");
        /// });
        /// // CSV content after mutation: "header2,header3\nvalue2,value3"
        /// ]]>
        /// </code>
        /// </example>
        void Mutate(Action<CSVMutationScope> mutations, bool saveChanges = true, bool safe = false);


        /// <summary>
        /// Performs asynchronous mutations on the CSV content using the provided delegate.
        /// </summary>
        /// <param name="mutations">An async delegate that takes a <see cref="CSVMutationScope"/> and performs the desired mutations on the CSV content asynchronously.</param>
        /// <param name="saveChanges">A boolean flag indicating whether to save the changes made by the mutations. If set to true, the method will update the internal CSV content representation. Default value is true.</param>
        /// <param name="safe">Determines whether to operate on a clone. Only useful is you don't save change</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// This example demonstrates how to use the MutateAsync method to add and remove a header.
        /// <code>
        /// <![CDATA[
        /// var csvManipulator = new CsvManipulator("header1,header2\nvalue1,value2");
        /// await csvManipulator.MutateAsync(async scope =>
        /// {
        ///     // Perform async operations here (if needed)
        ///     await Task.Delay(1000);
        ///     await easyCsv.RemoveUnusedHeadersAsync<Person>();
        ///
        ///     // CSV content before mutation: "header1,header2\nvalue1,value2"
        ///     scope.AddColumn("header3", "value3");
        ///     scope.RemoveColumn("header1");
        /// });
        /// // CSV content after mutation: "header2,header3\nvalue2,value3"
        /// ]]>
        /// </code>
        /// </example>
        Task MutateAsync(Func<CSVMutationScope, Task> mutations, bool saveChanges = true, bool safe = false);



        /// <summary>
        /// Calculates file content asynchronously but runs mutations synchronously.
        /// </summary>
        /// <param name="mutations">An async delegate that takes a <see cref="CSVMutationScope"/> and performs the desired mutations on the CSV content asynchronously.</param>
        /// <param name="saveChanges">A boolean flag indicating whether to save the changes made by the mutations. If set to true, the method will update the internal CSV content representation. Default value is true.</param>
        /// <param name="safe">Determines whether to operate on a clone. Only useful is you don't save change</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// This example demonstrates how to use the MutateAsync method to add and remove a header.
        /// <code>
        /// <![CDATA[
        /// var csvManipulator = new CsvManipulator("header1,header2\nvalue1,value2");
        /// await csvManipulator.MutateAsync(async scope =>
        /// {
        ///     scope.AddColumn("header3", "value3");
        ///     scope.RemoveColumn("header1");
        /// });
        /// // CSV content after mutation: "header2,header3\nvalue2,value3"
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>Useful when you are only doing synchronous mutations but want the benefits of calculating the content string and bytes asynchronously</remarks>
        Task MutateAsync(Action<CSVMutationScope> mutations, bool saveChanges = true, bool safe = false);
    }
}