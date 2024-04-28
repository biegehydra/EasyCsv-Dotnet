using EasyCsv.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyCsv.Parsing;

public interface ICsvColumnProcessorWithExample : ICsvColumnProcessor
{
    public string DisplayName { get; }
    public string Description { get; }
    public List<Dictionary<string, string>> ExampleInputRows { get; } 
    public List<Dictionary<string, string>> ExampleOutputRows { get; } 
}

public interface ICsvColumnProcessor
{
    public string ColumnName { get; }
    /// <summary>
    /// Process row will be called for each row in the
    /// csv. You will be able to modify the value of
    /// a single cell. This cell is guaranteed to
    /// have the specified <see cref="ColumnName"/>
    /// </summary>
    /// <param name="cell"></param>
    public Task<OperationResult> ProcessCell(ICell cell);
}
public interface ICsvRowProcessor
{
    /// <summary>
    /// This function will be called for each row in the csv.
    /// </summary>
    /// <param name="row"></param>
    public Task<OperationResult> ProcessRow(CsvRow row);
}

public interface ICsvProcessor
{
    /// <summary>
    /// This function will be called for each row in the csv.
    /// </summary>
    /// <param name="csv"></param>
    Task<OperationResult> ProcessCsv(IEasyCsv csv);
}

public interface ICsvMerger
{
    public void Merge(IEasyCsv csv1, IEasyCsv csv2);
}

public interface ICell
{
    public string ColumnName { get; }
    public string Value { get; set; }
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