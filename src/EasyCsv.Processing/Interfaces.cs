using System;
using System.Collections.Generic;
using System.Linq;
using EasyCsv.Core;
using System.Threading.Tasks;

namespace EasyCsv.Processing;

public interface IFindDedupesOperation
{
    public string TitleText { get; }
    public bool MultiSelect { get; }
    public bool MustSelectRow { get; }
    public Func<string, string>? DuplicateValuePresenter { get; }
    public IAsyncEnumerable<DuplicateGrouping> YieldReturnDupes(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null, params (IEasyCsv Csv, int ReferenceCsvId)[] referenceCsvs);
}

public interface ICsvColumnProcessor : IColumnOperation
{
    /// <summary>
    /// Process row will be called for each row in the
    /// csv. You will be able to modify the value of
    /// a single cell. This cell is guaranteed to
    /// have the specified <see cref="ColumnName"/>
    /// </summary>
    /// <param name="cell"></param>
    public ValueTask<OperationResult> ProcessCell<TCell>(TCell cell) where TCell : ICell;
}
public interface ICsvRowProcessor
{
    /// <summary>
    /// This function will be called for each row in the csv.
    /// </summary>
    /// <param name="row"></param>
    public ValueTask<OperationResult> ProcessRow(CsvRow row);
}

public interface ICsvColumnDeleteEvaluator : IColumnOperation
{
    public ValueTask<OperationDeleteResult> EvaluateDelete<TCell>(TCell cell) where TCell : ICell;
}

public interface ICsvRowDeleteEvaluator
{
    /// <summary>
    /// This function will be called for each row in the csv.
    /// </summary>
    /// <param name="row"></param>
    public ValueTask<OperationDeleteResult> EvaluateDelete(CsvRow row);
}

public interface ICsvReferenceProcessor : IReferenceOperation
{
    ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, IEasyCsv referenceCsv, ICollection<int>? filteredRowIndexes = null);
}


public interface ICsvProcessor
{
    /// <summary>
    /// This function will be called for each row in the csv.
    /// </summary>
    /// <param name="csv"></param>
    ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null);
}


public interface ICsvMerger
{
    public ValueTask<IEasyCsv> Merge(IEasyCsv baseCsv, IEasyCsv additionalCsv);
}

public interface ICell
{
    public string ColumnName { get; }
    public object? Value { get; set; }
}

public interface IColumnOperation
{
    public string ColumnName { get; }
}
public interface IReferenceOperation
{
    public int ReferenceCsvId { get; }
}

public readonly struct DuplicateGrouping
{
    public string DuplicateValue { get; }
    public (int?, (int, CsvRow)[])[] Duplicates { get; }
    public DuplicateGrouping(string duplicateValue, IEnumerable<(int?, IEnumerable<(int, CsvRow)>)> duplicatesGroupedByReferenceCsvId)
    {
        if (string.IsNullOrWhiteSpace(duplicateValue))
        {
            throw new ArgumentException("Duplicate value cannot be null or white space", nameof(duplicateValue));
        }
        DuplicateValue = duplicateValue;
        if (duplicatesGroupedByReferenceCsvId == null!)  throw new ArgumentException("duplicatesGroupedByReferenceCsvId cannot be null");
        Duplicates = duplicatesGroupedByReferenceCsvId.Select(x => (x.Item1, x.Item2.ToArray())).ToArray();
    }
}

public readonly struct OperationResult
{
    public OperationResult(bool success, string? message = null, double progress = 0)
    {
        Success = success;
        Message = message;
        Progress = progress;
    }
    public double Progress { get; }
    public bool Success { get; }
    public string? Message { get; } 
}

public readonly struct OperationDeleteResult
{
    public OperationDeleteResult(bool success, bool delete, string? message = null, double progress = 0)
    {
        Success = success;
        Message = message;
        Delete = delete;
        Progress = progress;
    }
    public bool Success { get; }
    public double Progress { get; }
    public bool Delete { get; }
    public string? Message { get; }
}

public readonly struct AggregateOperationDeleteResult
{
    public AggregateOperationDeleteResult(bool success, int deleted, string? message = null)
    {
        Success = success;
        Message = message;
        Deleted = deleted;
    }
    public bool Success { get; }
    public int Deleted { get; }
    public string? Message { get; }
}