using System;
using EasyCsv.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyCsv.Core.Extensions;
using EasyCsv.Processing.Strategies;

[assembly: InternalsVisibleTo("EasyCsv.Components")]
namespace EasyCsv.Processing;
public class StrategyRunner
{
    private int _currentIndex = -1;
    public int CurrentIndex => _currentIndex;
    public int? CurrentRowEditIndex => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].CurrentRowEditIndex.Value : null;
    public IEasyCsv? CurrentCsv => IsCacheIndexValid(_currentIndex) ? _steps[_currentIndex].Csv : null;
    public string[]? CurrentCsvColumnNames => IsCacheIndexValid(_currentIndex) ? _steps[_currentIndex].ColumnNames : null;
    public HashSet<string>? CurrentTags => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].Tags : null;
    internal Dictionary<CsvRow, int>? CurrentOriginalRowIndexes => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].OriginalRowIndexes : null;
    internal List<IReversibleEdit>? CurrentReversibleEdits => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].ReversibleEdits : null;
    private readonly List<(IEasyCsv Csv, string FileName)> _referenceCsvs = new();
    public IReadOnlyList<(IEasyCsv Csv, string FileName)> ReferenceCsvs => _referenceCsvs;
    internal readonly List<(IEasyCsv Csv, string[] ColumnNames, HashSet<string> Tags, List<IReversibleEdit> ReversibleEdits, Dictionary<CsvRow, int> OriginalRowIndexes, CurrentRowEditIndex CurrentRowEditIndex)> _steps = new();
    public StrategyRunner(IEasyCsv baseCsv)
    {
        var clone = baseCsv.Clone(); 
        _steps.Add((clone, clone.ColumnNames()!, AllUniqueTags(clone), new List<IReversibleEdit>(), OriginalRowIndexes(clone), new CurrentRowEditIndex()));
        SetCurrentIndexSafe(0);
    }

    public OperationResult AddRows(ICollection<CsvRow> rowsToAdd)
    {
        if (rowsToAdd?.Count is not > 0 || CurrentCsv == null) return new OperationResult(false, "Rows to add was empty or the current csv is null.");
        var (operationResult, rowsPreparedToAdd) = AddRowsHelper.CreateSameStructureRows(rowsToAdd, CurrentCsv!);
        if (!operationResult.Success) return operationResult;
        var addRowsEdit = new AddRowsEdit(rowsPreparedToAdd);
        AddReversibleEdit(addRowsEdit);
        foreach (var row in rowsPreparedToAdd)
        {
            CurrentOriginalRowIndexes![row] = CurrentOriginalRowIndexes.Count;
        }
        return operationResult;
    }

    public bool AddReversibleEdit(IReversibleEdit reversibleEdit)
    {
        if (CurrentCsv == null) return false;
        while (_steps[_currentIndex].CurrentRowEditIndex.Value != _steps[_currentIndex].ReversibleEdits.Count - 1)
        {
            _steps[_currentIndex].ReversibleEdits.Remove(_steps[_currentIndex].ReversibleEdits[_steps[_currentIndex].ReversibleEdits.Count - 1]);
        }
        _steps[_currentIndex].ReversibleEdits.Add(reversibleEdit);
        GoForwardEdit();
        return true;
    }

    public async ValueTask<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvColumnDeleteEvaluator was null");
        List<CsvRow> rowsToDelete = new ();
        string columnName = evaluateDelete.ColumnName;
        foreach (var (row, _) in CurrentCsv.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            var operationResult = await evaluateDelete.EvaluateDelete(new RowCell(columnName, row[columnName]));
            if (operationResult.Delete)
            {
                rowsToDelete.Add(row);
            }
            if (operationResult.Success == false)
            {
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }

        var deleteRowsEdit = new DeleteRowsEdit(rowsToDelete);
        AddReversibleEdit(deleteRowsEdit);
        string message = $"Deleted {rowsToDelete.Count} rows";
        return new AggregateOperationDeleteResult(true, rowsToDelete.Count, message);
    }

    public async ValueTask<AggregateOperationDeleteResult> RunRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvRowDeleteEvaluator was null");

        List<CsvRow> rowsToDelete = new ();
        foreach (var (row, index) in CurrentCsv.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            var operationResult = await evaluateDelete.EvaluateDelete(row, index);
            if (operationResult.Delete)
            {
                rowsToDelete.Add(row);
            }
            if (operationResult.Success == false)
            {
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }

        var deleteRowsEdit = new DeleteRowsEdit(rowsToDelete);
        AddReversibleEdit(deleteRowsEdit);
        string message = $"Deleted {rowsToDelete.Count} rows";
        return new AggregateOperationDeleteResult(true, rowsToDelete.Count, message);
    }

    public async ValueTask<OperationResult> RunReferenceStrategy(ICsvReferenceProcessor csvReferenceProcessor, ICollection<int>? filteredRowIds)
    {
        if (csvReferenceProcessor == null!) return new OperationResult(false, "CsvReferenceProcessor was null");
        int referenceId = csvReferenceProcessor.ReferenceCsvId;
        if (referenceId < 0 || referenceId >= ReferenceCsvs.Count)
        {
            return new OperationResult(false, "Invalid reference id");
        }
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentCsv.Clone();
        var operationResult = await csvReferenceProcessor.ProcessCsv(clone, ReferenceCsvs[referenceId].Csv, filteredRowIds);
        if (operationResult.Success)
        {
            AddToTimeline(clone);
        }
        return operationResult;
    }

    public async ValueTask<OperationResult> RunColumnStrategy(ICsvColumnProcessor columnProcessor, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var columnName = columnProcessor.ColumnName;
        List<CellEdit> cellEdits = new List<CellEdit>(CurrentCsv.CsvContent.Count / 8);
        foreach (var (row, _) in CurrentCsv.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            object? originalValue = row[columnName];
            var rowCell = new RowCell(columnName, originalValue);
            var operationResult = await columnProcessor.ProcessCell(ref rowCell);
            if (operationResult.Success == false)
            {
                return operationResult;
            }
            if (rowCell.ValueChanged)
            {
                cellEdits.Add(new CellEdit(row, originalValue, rowCell.Value));
            }
        }

        if (cellEdits.Count > 0)
        {
            var columnEdit = new MakeColumnEdit(cellEdits, columnName);
            AddReversibleEdit(columnEdit);
        }

        return new OperationResult(true);
    }

    public async ValueTask<OperationResult> RunRowStrategy(ICsvRowProcessor rowProcessor, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentCsv.Clone();
        foreach (var (row, index) in clone.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            var operationResult = await rowProcessor.ProcessRow(row, index);
            if (operationResult.Success == false)
            {
                return operationResult;
            }
        }
        AddToTimeline(clone);
        return new OperationResult(true);
    }

    public async ValueTask<OperationResult> RunCsvStrategy(IFullCsvProcessor fullCsvProcessor, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentCsv.Clone();
        var operationResult = await fullCsvProcessor.ProcessCsv(clone, filteredRowIds);
        if (operationResult.Success)
        {
            AddToTimeline(clone);
        }

        return operationResult;
    }

    public void AddReference(IEasyCsv csv, string name)
    {
        if (csv == null! || string.IsNullOrWhiteSpace(name)) return;
        _referenceCsvs.Add((csv, name));
    }

    public void AddToTimeline(IEasyCsv csv)
    {
        // If the timeline looks like this,
        // and we want to add to the timeline
        // with the ^ representing _currentIndex
        // 1->2->3->4->5
        //       ^
        // Then we want to remove 4 and 5, like so
        // 1->2->3->6
        while (_currentIndex != _steps.Count - 1)
        {
            _steps.Remove(_steps[_steps.Count - 1]);
        }
        _steps.Add((csv, csv.ColumnNames()!, AllUniqueTags(csv), new List<IReversibleEdit>(),  OriginalRowIndexes(csv), new CurrentRowEditIndex()));
        SetCurrentIndexSafe(_steps.Count - 1);
    }

    public void SortCurrentBackToOriginalOrder()
    {
        if (CurrentCsv == null) return;
        var originalRowIndexes = CurrentOriginalRowIndexes!;
        CurrentCsv.CsvContent.Sort((one, two) =>
        {
            if (!originalRowIndexes.TryGetValue(one, out var indexX))
            {
                indexX = int.MaxValue;
            }

            if (!originalRowIndexes.TryGetValue(two, out var indexY))
            {
                indexY = int.MaxValue;
            }
            return indexX.CompareTo(indexY);
        });
    }

    private HashSet<string> AllUniqueTags(IEasyCsv? easyCsv)
    {
        var uniqueTags = new HashSet<string>();
        if (easyCsv == null!) return uniqueTags;
        foreach (var row in easyCsv.CsvContent)
        {
            var tags = row.ProcessingTags();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    uniqueTags.Add(tag);
                }
            }
        }
        return uniqueTags;
    }

    private Dictionary<CsvRow, int> OriginalRowIndexes(IEasyCsv? easyCsv)
    {
        var originalIndexes = new Dictionary<CsvRow, int>();
        if (easyCsv == null!) return originalIndexes;
        return easyCsv.CsvContent.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i);
    }

    // Current Row Edit is the one we are currently after
    public bool GoForwardEdit()
    {
        if (Utils.IsValidIndex(CurrentRowEditIndex + 1, CurrentReversibleEdits?.Count ?? -1))
        {
            var nextRowEdit = CurrentReversibleEdits![CurrentRowEditIndex!.Value + 1];
            nextRowEdit.DoEdit(CurrentCsv!);
            _steps[_currentIndex].CurrentRowEditIndex.Value += 1;
            return true;
        }
        return false;
    }

    public bool GoBackEdit()
    {
        if (Utils.IsValidIndex(CurrentRowEditIndex, CurrentReversibleEdits?.Count ?? -1))
        {
            var currentRowEdit = CurrentReversibleEdits![CurrentRowEditIndex!.Value];
            currentRowEdit.UndoEdit(CurrentCsv!);
            _steps[_currentIndex].CurrentRowEditIndex.Value -= 1;
            return true;
        }
        return false;
    }

    public bool GoBackStep()
    {
        return SetCurrentIndexSafe(_currentIndex - 1);
    }

    public bool GoForwardStep()
    {
        return SetCurrentIndexSafe(_currentIndex + 1);
    }

    private bool SetCurrentIndexSafe(int index)
    {
        if (IsCacheIndexValid(index))
        {
            _currentIndex = index;
            return true;
        }
        return false;
    }
#if NETSTANDARD2_0
        public bool IsCacheIndexValid(int? index)
#else
    public bool IsCacheIndexValid([NotNullWhen(true)] int? index)
#endif
    {
        return Utils.IsValidIndex(index, _steps.Count);
    }

#if NETSTANDARD2_0
        public bool IsReferenceIndexValid(int? index)
#else
    public bool IsReferenceIndexValid([NotNullWhen(true)] int? index)
#endif
    {
        return Utils.IsValidIndex(index, ReferenceCsvs.Count);
    }
}

public readonly record struct CellEdit
{
    public CellEdit(CsvRow row, object? beforeValue, object? afterValue)
    {
        Row = row;
        BeforeValue = beforeValue;
        AfterValue = afterValue;
    }

    public CsvRow Row { get; }
    public object? BeforeValue { get; }
    public object? AfterValue { get; }
};
public class MakeColumnEdit : IReversibleEdit
{
    private readonly ICollection<CellEdit> _cellEdits;
    private readonly string _columnName;

    public MakeColumnEdit(ICollection<CellEdit> cellEdits, string columnName)
    {
        _cellEdits = cellEdits;
        _columnName = columnName;
    }

    public void DoEdit(IEasyCsv csv)
    {
        foreach (var cellEdit in _cellEdits)
        {
            cellEdit.Row[_columnName] = cellEdit.AfterValue;
        }
    }

    public void UndoEdit(IEasyCsv csv)
    {
        foreach (var cellEdit in _cellEdits)
        {
            cellEdit.Row[_columnName] = cellEdit.BeforeValue;
        }
    }
}

public class AddRowsEdit : IReversibleEdit
{
    internal readonly ICollection<CsvRow> _rowsToAdd;

    public AddRowsEdit(ICollection<CsvRow> rowsToAdd)
    {
        _rowsToAdd = rowsToAdd;
    }

    public void DoEdit(IEasyCsv csv)
    {
        foreach (var row in _rowsToAdd)
        {
            csv.CsvContent.Add(row);
        }
    }

    public void UndoEdit(IEasyCsv csv)
    {
        foreach (var row in _rowsToAdd)
        {
            csv.CsvContent.Remove(row);
        }
    }
}

public class DeleteRowsEdit : IReversibleEdit
{
    private readonly ICollection<CsvRow> _rowsToDelete;

    public DeleteRowsEdit(ICollection<CsvRow> rowsToDelete)
    {
        _rowsToDelete = rowsToDelete;
    }

    public void DoEdit(IEasyCsv csv)
    {
        foreach (var row in _rowsToDelete)
        {
            csv.CsvContent.Remove(row);
        }
    }

    public void UndoEdit(IEasyCsv csv)
    {
        foreach (var row in _rowsToDelete)
        {
            csv.CsvContent.Add(row);
        }
    }
}

public class ModifyRowEdit : IReversibleEdit
{
    public ModifyRowEdit(CsvRow row, CsvRow rowClone, CsvRow rowAfterOperation)
    {
        Row = row;
        RowClone = rowClone;
        RowAfterOperation = rowAfterOperation;
    }

    public CsvRow Row { get; }
    public CsvRow RowClone { get; }
    public CsvRow RowAfterOperation { get; }
    public void DoEdit(IEasyCsv csv)
    {
#if DEBUG
        var equals = Row.ValuesEqual(RowClone);
        if (!equals)
        {
            throw new Exception("Should've equalled Before Row");
        }
#endif
        RowAfterOperation.MapValuesTo(Row);
    }
    public void UndoEdit(IEasyCsv csv)
    {
#if DEBUG
        var equals = Row.ValuesEqual(RowAfterOperation);
        if (!equals)
        {
            throw new Exception("Should've equalled After Row");
        }
#endif
        RowClone.MapValuesTo(Row);
    }
}
public class AddTagEdit : IReversibleEdit
{
    public AddTagEdit(CsvRow row, string tagToAdd, int tagColumnIndex = -1)
    {
        Row = row;
        TagToAdd = tagToAdd;
        TagColumnIndex = tagColumnIndex;
    }
    public CsvRow Row { get; }
    public string TagToAdd { get; }
    public int TagColumnIndex { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (TagColumnIndex >= 0)
        {
            Row.AddProcessingTag(TagColumnIndex, TagToAdd);
        }
        else
        {
            Row.AddProcessingTag(TagToAdd);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (TagColumnIndex >= 0)
        {
            Row.AddProcessingTag(TagColumnIndex, TagToAdd);
        }
        else
        {
            Row.AddProcessingTag(TagToAdd);
        }
    }
}
public class DeleteRowEdit : IReversibleEdit
{
    private readonly CsvRow _row;

    public DeleteRowEdit(CsvRow row)
    {
        _row = row;
    }
    public void DoEdit(IEasyCsv csv)
    {
        csv.CsvContent.Remove(_row);
    }
    public void UndoEdit(IEasyCsv csv)
    {
        csv.CsvContent.Add(_row);
    }
}

public class RemoveTagEdit : IReversibleEdit
{
    public RemoveTagEdit(CsvRow row, string tagToRemove, int tagColumnIndex = -1)
    {
        Row = row;
        TagToRemove = tagToRemove;
        TagColumnIndex = tagColumnIndex;
    }

    public CsvRow Row { get; }
    public string TagToRemove { get; }
    public int TagColumnIndex { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (TagColumnIndex >= 0)
        {
            Row.RemoveProcessingTag(TagToRemove);
        }
        else
        {
            Row.RemoveProcessingTag(TagColumnIndex, TagToRemove);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (TagColumnIndex >= 0)
        {
            Row.AddProcessingTag(TagToRemove);
        }
        else
        {
            Row.AddProcessingTag(TagColumnIndex, TagToRemove);
        }
    }
}
public class RemoveReferenceEdit : IReversibleEdit
{
    public RemoveReferenceEdit(CsvRow row, int referenceCsvId, int referenceRowId, int referencesColumnIndex = -1)
    {
        Row = row;
        ReferenceCsvId = referenceCsvId;
        ReferencesColumnIndex = referencesColumnIndex;
        ReferenceRowId = referenceRowId;
    }

    public CsvRow Row { get; }
    public int ReferenceCsvId { get; }
    public int ReferencesColumnIndex { get; }
    public int ReferenceRowId { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            Row.RemoveProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            Row.RemoveProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            Row.AddProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            Row.AddProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
}
public class AddReferenceEdit : IReversibleEdit
{
    public AddReferenceEdit(CsvRow row, int referenceCsvId, int referenceRowId, int referencesColumnIndex = -1)
    {
        Row = row;
        ReferenceCsvId = referenceCsvId;
        ReferencesColumnIndex = referencesColumnIndex;
        ReferenceRowId = referenceRowId;
    }

    public CsvRow Row { get; }
    public int ReferenceCsvId { get; }
    public int ReferencesColumnIndex { get; }
    public int ReferenceRowId { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            Row.AddProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            Row.AddProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            Row.RemoveProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            Row.RemoveProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
}

public interface IReversibleEdit
{
    void DoEdit(IEasyCsv csv);
    void UndoEdit(IEasyCsv csv);
}

internal class CurrentRowEditIndex
{
    public int Value { get; set; } = -1;
}

public struct RowCell : ICell
{
    public string ColumnName { get; }
    internal bool ValueChanged { get; set; }
    public RowCell(string columnName, object? value)
    {
        ColumnName = columnName;
        _value = value;
    }

    private object? _value;
    public object? Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<object?>.Default.Equals(_value, value))
            {
                _value = value;
                ValueChanged = true;
            }
        }
    }
}
