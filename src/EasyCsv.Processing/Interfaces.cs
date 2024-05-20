using System;
using System.Collections.Generic;
using System.Linq;
using EasyCsv.Core;
using System.Threading.Tasks;

namespace EasyCsv.Processing;

public interface IFindDupesOperation : ICsvProcessor
{
    /// <summary>
    /// Optional parameter to make it so the UI shows, "found in 'column name'"
    /// </summary>
    public string? ColumnName { get; }
    /// <summary>
    /// Controls whether users can select multiple rows to keep
    /// </summary>
    public bool MultiSelect { get; }
    /// <summary>
    /// Controls whether a user is required to select a row to keep
    /// </summary>
    public bool MustSelectRow { get; }
    /// <summary>
    /// Optional parameter to alter the value of the string before displaying it to
    /// the user
    /// </summary>
    public Func<string, string>? DuplicateValuePresenter { get; }
    /// <summary>
    /// If multi-select is False, this will be used to auto select a row to keep.
    /// Note, this overrides ResolveDuplicatesAutoSelect.FirstRow
    /// </summary>
    public Func<CsvRow[], CsvRow?>? AutoSelectRow { get; }
    /// <summary>
    /// If multi-select is True, this will be used to auto select rows to keep.
    /// Note, this overrides ResolveDuplicatesAutoSelect.FirstRow
    /// </summary>
    public Func<CsvRow[], CsvRow[]?>? AutoSelectRows { get; }
    /// <summary>
    /// Yield return dupes should not alter <paramref name="csv"/>. It should only search it,
    /// and return grouped of rows that are considered duplicates so that the users
    /// can pick one to keep
    /// </summary>
    /// <param name="csv">The csv to find duplicates in</param>
    /// <param name="filteredRowIndexes">The row indexes that the user has filtered the csv down to. Not all operations have to adhere to these indexes. See <see cref="ICsvProcessor.OperatesOnlyOnFilteredRows"/> to check if an operation adheres to these indexes.</param>
    /// <param name="referenceCsvs">The reference csvs given are the ones specified when CsvProcessingStepper.PerformDedupe is called</param>
    /// <returns>Groups of duplicates for the users to select one to kep</returns>
    public IAsyncEnumerable<DuplicateGrouping> YieldReturnDupes(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null, params (IEasyCsv Csv, int ReferenceCsvId)[] referenceCsvs);
}

public interface ICsvColumnProcessor : IColumnOperation, IProvideCompletedTextStrategy
{
    /// <summary>
    /// Process row will be called for each row in the
    /// csv. You will be able to modify the value of
    /// a single cell. This cell is guaranteed to
    /// have the specified <see cref="IColumnOperation.ColumnName"/>
    /// </summary>
    /// <param name="cell">The current cell this processor is operating on</param>
    /// <returns>A result saying if the operation was successful. If the operation was not successful, the <see cref="StrategyRunner"/> will cancel the operation and undo any changes</returns>
    public ValueTask<OperationResult> ProcessCell<TCell>(ref TCell cell) where TCell : ICell;
}
public interface ICsvRowProcessor : ICsvProcessor, IProvideCompletedTextStrategy
{
    /// <summary>
    /// This function will be called for each row in the csv. You will be able to modify
    /// this row freely. However, anything you do to a single row that affects the "shape"
    /// of the that row, e.g. adding/removing/moving columns around, you must do to all rows.
    /// Maintaining the shape of the csv as a whole is the job of the processor, not the
    /// <see cref="StrategyRunner"/>
    /// </summary>
    /// <param name="row">The current row this processor is operating on</param>
    /// <returns>
    /// An operation result with a Success flag.
    /// If Success is False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes
    /// </returns>
    public ValueTask<OperationResult> ProcessRow(CsvRow row);
}

public interface ICsvColumnDeleteEvaluator : IColumnOperation
{
    /// <summary>
    /// This function will be called for each row in the csv. You will be able to look at
    /// all the cells in a column and determine if that row should be deleted based on
    /// the value of the row. 
    /// </summary>
    /// <param name="cell">The cell of a particular row and column that this evaluator is currently determining whether to delete</param>
    /// <returns>
    /// A delete result with a Success and Delete flag.
    /// If Delete is True, the row that <paramref name="cell"/> is a part of will be deleted.
    /// If Success is False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes
    /// </returns>
    public ValueTask<OperationDeleteResult> EvaluateDelete<TCell>(TCell cell) where TCell : IReadonlyCell;
}

public interface ICsvRowDeleteEvaluator : ICsvProcessor
{
    /// <summary>
    /// This function will be called for each row in the csv. You will be able to look at
    /// all the cells in the csv and determine if they should be deleted. You should
    /// not alter the shape of the csv or any of the values during the operation
    /// </summary>
    /// <param name="row">The row that this evaluator is determining whether to delete</param>
    /// <returns>
    /// A delete result with a Success and Delete flag.
    /// If Delete is True, <paramref name="row"/> will be deleted.
    /// If Success is False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes
    /// </returns>
    public ValueTask<OperationDeleteResult> EvaluateDelete(CsvRow row);
}

public interface ICsvReferenceProcessor : IReferenceOperation
{
    /// <summary>
    /// During this operation, the processor can look at the <paramref name="csv"/> and <paramref name="referenceCsv"/>.
    /// It can also make changes to <paramref name="csv"/> but it is responsible for saving those changes and
    /// maintaining the shape of the csv in all rows. 
    /// You can not alter <paramref name="referenceCsv"/> in any way shape or form during this operation.
    /// </summary>
    /// <param name="csv">The csv that this processor is operating on</param>
    /// <param name="referenceCsv">A csv this processor can look at and use to <see cref="CsvRow.AddProcessingReferences(int, IEnumerable{int})"/> to rows</param>
    /// <param name="filteredRowIndexes">The row indexes that the user has filtered the csv down to. Not all operations have to adhere to these indexes. See <see cref="ICsvProcessor.OperatesOnlyOnFilteredRows"/> to check if an operation adheres to these indexes.</param>
    ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, IEasyCsv referenceCsv, ICollection<int>? filteredRowIndexes = null);
}


public interface IFullCsvProcessor : ICsvProcessor
{
    /// <summary>
    /// During this operation, the processor can look at and modify <paramref name="csv"/>,
    /// but it is responsible for saving those changes and
    /// maintaining the shape of the csv in all rows. 
    /// </summary>
    /// <param name="csv">The csv that this processor is operating on</param>
    /// <param name="filteredRowIndexes">The row indexes that the user has filtered the csv down to. Not all operations have to adhere to these indexes. See <see cref="ICsvProcessor.OperatesOnlyOnFilteredRows"/> to check if an operation adheres to these indexes.</param>
    ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null);
}


public interface ICsvMerger : ICsvProcessor
{
    /// <summary>
    /// During this operation, the processor can look at and modify both <paramref name="csv1"/>
    /// and <paramref name="csv2"/>. By choosing one of them, or creating a new csv,
    /// this function is expected to return a merged version of the 2 csvs.
    /// </summary>
    /// <param name="csv1">The first csv that this processor is given to merge</param>
    /// <param name="csv2">The second csv this processor is given to merge.</param>
    public ValueTask<IEasyCsv> Merge(IEasyCsv csv1, IEasyCsv csv2);
}

public interface ICsvProcessor
{
    /// <summary>
    /// A flag to indicate whether the operation of this <see cref="ICsvProcessor"/> only operates on
    /// the filtered row. When working with the CsvProcessingStepper, there are flags on the StrategyItem
    /// to control how this gets indicated on the UI 
    /// </summary>
    public bool OperatesOnlyOnFilteredRows { get; }
}

/// <summary>
/// A readonly representation of a cell in an <see cref="IEasyCsv"/>
/// </summary>
public interface IReadonlyCell
{
    /// <summary>
    /// The column this cell is a part of
    /// </summary>
    public string ColumnName { get; }
    /// <summary>
    /// The value of this cell in the csv
    /// </summary>
    public object? Value { get; }
}
/// <summary>
/// A read/write representation of a cell in an <see cref="IEasyCsv"/>
/// </summary>
public interface ICell : IReadonlyCell
{
    /// <summary>
    /// The value of this cell in the csv
    /// </summary>
    public new object? Value { get; set; }
}

public interface IColumnOperation : ICsvProcessor
{
    /// <summary>
    /// The column that this operates on. For column processors, this controls which cell in a particular
    /// row that the processor operates on
    /// </summary>
    public string ColumnName { get; }
}
public interface IReferenceOperation : ICsvProcessor
{
    /// <summary>
    /// The Id of the reference csv in the CsvProcessingStepper 
    /// </summary>
    public int ReferenceCsvId { get; }
}

public interface IProvideCompletedTextStrategy
{

}

public readonly struct DuplicateGrouping
{
    /// <summary>
    /// The value seen in multiple rows
    /// </summary>
    public string DuplicateValue { get; }
    /// <summary>
    /// The rows that are duplicates and their indexes
    /// </summary>
    public (int, CsvRow)[] Duplicates { get; }
    /// <param name="duplicateValue">The value seen in multiple rows</param>
    /// <param name="duplicateRowsWithIndexes">The rows that are duplicates and their indexes</param>
    /// <exception cref="ArgumentException"></exception>
    public DuplicateGrouping(string duplicateValue, IEnumerable<(int, CsvRow)> duplicateRowsWithIndexes)
    {
        if (string.IsNullOrWhiteSpace(duplicateValue))
        {
            throw new ArgumentException("Duplicate value cannot be null or white space", nameof(duplicateValue));
        }
        DuplicateValue = duplicateValue;
        if (duplicateRowsWithIndexes == null!)  throw new ArgumentException("duplicatesGroupedByReferenceCsvId cannot be null");
        Duplicates = duplicateRowsWithIndexes.ToArray();
    }
}

public readonly struct OperationResult : IOperationResult
{
    /// <param name="success">If False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes</param>
    /// <param name="message">A message to accompany the result. For operations that operate on the full csv, these messages
    /// will be added as snack bars</param>
    /// <param name="progress">Not an implemented feature yet, shows the progress of the operation</param>
    public OperationResult(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }
    /// <summary>
    /// If False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes
    /// </summary>
    public bool Success { get; }
    /// <summary>
    /// A message to accompany the result. For operations that operate on the full csv, these messages
    /// will be added as snack bars
    /// </summary>
    public string? Message { get; } 
}

public readonly struct OperationDeleteResult : IOperationResult
{
    /// <param name="success">If False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes</param>
    /// <param name="delete">A flag to indicate if a row should be deleted</param>
    /// <param name="message">A message to accompany the result. For operations that operate on the full csv, these messages
    /// will be added as snack bars by the CsvProcessingStepper</param>
    /// <param name="progress">Not an implemented feature yet, shows the progress of the operation</param>
    public OperationDeleteResult(bool success, bool delete, string? message = null)
    {
        Success = success;
        Message = message;
        Delete = delete;
    }    
    /// <summary>
    /// If False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes
    /// </summary>
    public bool Success { get; }
    /// <summary>
    /// A flag to indicate if a row should be deleted
    /// </summary>
    public bool Delete { get; }
    /// <summary>
    /// A message to accompany the result. For operations that operate on the full csv, these messages
    /// will be added as snack bars by the CsvProcessingStepper
    /// </summary>
    public string? Message { get; }
}

internal interface IOperationResult
{
    public string? Message { get; }
    public bool Success { get; }
}

public readonly struct AggregateOperationDeleteResult : IOperationResult
{
    /// <param name="success">If False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes</param>
    /// <param name="deleted">An integer indicating how many rows were delete</param>
    /// <param name="message">A message to accompany the result. This will be added to the snackbar by the CsvProcessingStepper </param>
    public AggregateOperationDeleteResult(bool success, int deleted, string? message = null)
    {
        Success = success;
        Message = message;
        Deleted = deleted;
    }
    /// <summary>
    /// If False, the <see cref="StrategyRunner"/> will cancel the operation and rollback changes
    /// </summary>
    public bool Success { get; }
    /// <summary>
    /// An integer indicating how many rows were delete
    /// </summary>
    public int Deleted { get; }
    /// <summary>
    /// A message to accompany the result. This will be added to the snackbar by the CsvProcessingStepper 
    /// </summary>
    public string? Message { get; }
}