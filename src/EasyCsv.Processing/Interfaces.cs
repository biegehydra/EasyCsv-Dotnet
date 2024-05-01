using EasyCsv.Core;
using System.Threading.Tasks;

namespace EasyCsv.Processing;

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
    public Task<OperationDeleteResult> EvaluateDelete(CsvRow row);
}

public interface ICsvReferenceProcessor : IReferenceOperation
{
    ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, IEasyCsv referenceCsv);
}


public interface ICsvProcessor
{
    /// <summary>
    /// This function will be called for each row in the csv.
    /// </summary>
    /// <param name="csv"></param>
    ValueTask<OperationResult> ProcessCsv(IEasyCsv csv);
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

public readonly struct OperationResult
{
    public OperationResult(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }
    public bool Success { get; }
    public string? Message { get; } 
}

public readonly struct OperationDeleteResult
{
    public OperationDeleteResult(bool success, bool delete, string? message = null)
    {
        Success = success;
        Message = message;
        Delete = delete;
    }
    public bool Success { get; }
    public bool Delete { get; }
    public string? Message { get; }
}