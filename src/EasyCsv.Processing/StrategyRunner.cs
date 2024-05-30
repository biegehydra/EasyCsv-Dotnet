using System;
using EasyCsv.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyCsv.Core.Extensions;
using EasyCsv.Processing.Strategies;
using System.Text;

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

    public void RecomputeCurrentColumnNames()
    {
        if (CurrentCsv == null) return;
        var newColumnNames = CurrentCsv.ColumnNames()!;
        var temp = _steps[CurrentIndex];
        _steps[CurrentIndex] = (temp.Csv, newColumnNames, temp.Tags, temp.ReversibleEdits, temp.OriginalRowIndexes, temp.CurrentRowEditIndex);
    }

    public OperationResult AddRows(ICollection<CsvRow> rowsToAdd)
    {
        if (rowsToAdd?.Count is not > 0 || CurrentCsv == null) return new OperationResult(false, "Rows to add was empty or the current csv is null.");
        var (operationResult, rowsPreparedToAdd) = AddRowsEditHelper.CreateSameStructureRows(rowsToAdd, CurrentCsv!);
        if (!operationResult.Success || rowsPreparedToAdd.Count == 0) return operationResult;
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

    public async ValueTask<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds, Func<double, Task>? onProgressFunc = null)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvColumnDeleteEvaluator was null");
        List<CsvRow> rowsToDelete = new ();
        string columnName = evaluateDelete.ColumnName;
        int total = filteredRowIds == null ? CurrentCsv.CsvContent.Count : filteredRowIds.Count;
        int processed = 0;
        foreach (var row in CurrentCsv.CsvContent.FilterByIndexes(filteredRowIds))
        {
            var operationResult = await evaluateDelete.EvaluateDelete(new RowCell(columnName, row[columnName]));
            processed++;
            if (onProgressFunc != null)
            {
                await onProgressFunc((double) processed / total);
            }
            if (operationResult.Delete)
            {
                rowsToDelete.Add(row);
            }
            if (operationResult.Success == false)
            {
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }

        if (rowsToDelete.Count > 0)
        {
            var deleteRowsEdit = new DeleteRowsEdit(rowsToDelete);
            AddReversibleEdit(deleteRowsEdit);
        }

        string message = $"Deleted {rowsToDelete.Count} rows";
        return new AggregateOperationDeleteResult(true, rowsToDelete.Count, message);
    }

    public async ValueTask<AggregateOperationDeleteResult> RunRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds, Func<double, Task>? onProgressFunc = null)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvRowDeleteEvaluator was null");

        List<CsvRow> rowsToDelete = new ();
        int total = filteredRowIds == null ? CurrentCsv.CsvContent.Count : filteredRowIds.Count;
        int processed = 0;
        foreach (var row in CurrentCsv.CsvContent.FilterByIndexes(filteredRowIds))
        {
            var operationResult = await evaluateDelete.EvaluateDelete(row);
            processed++;
            if (onProgressFunc != null)
            {
                await onProgressFunc((double)processed / total);
            }
            if (operationResult.Delete)
            {
                rowsToDelete.Add(row);
            }
            if (operationResult.Success == false)
            {
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }

        if (rowsToDelete.Count > 0)
        {
            var deleteRowsEdit = new DeleteRowsEdit(rowsToDelete);
            AddReversibleEdit(deleteRowsEdit);
        }

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

    public async ValueTask<OperationResult> RunColumnStrategy(ICsvColumnProcessor columnProcessor, ICollection<int>? filteredRowIds, Func<double, Task>? onProgressFunc = null)
    {
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var columnName = columnProcessor.ColumnName;
        List<CellEdit> cellEdits = new List<CellEdit>(CurrentCsv.CsvContent.Count / 8);
        int total = filteredRowIds?.Count ?? CurrentCsv.CsvContent.Count;
        int processed = 0;
        foreach (var row in CurrentCsv.CsvContent.FilterByIndexes(filteredRowIds))
        {
            object? originalValue = row[columnName];
            var rowCell = new RowCell(columnName, originalValue);
            var operationResult = await columnProcessor.ProcessCell(ref rowCell);
            processed++;
            if (onProgressFunc != null)
            {
                await onProgressFunc((double)processed / total);
            }
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
            var columnEdit = new PerformMultipleCellEdits(cellEdits, columnName);
            AddReversibleEdit(columnEdit);
        }

        return new OperationResult(true);
    }

    public async ValueTask<OperationResult> RunRowStrategy(ICsvRowProcessor rowProcessor, ICollection<int>? filteredRowIds, Func<double, Task>? onProgressFunc = null)
    {
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentCsv.Clone();
        int total = filteredRowIds?.Count ?? CurrentCsv.CsvContent.Count;
        int processed = 0;
        foreach (var row in clone.CsvContent.FilterByIndexes(filteredRowIds))
        {
            var operationResult = await rowProcessor.ProcessRow(row);
            processed++;
            if (onProgressFunc != null)
            {
                await onProgressFunc((double)processed / total);
            }
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

    public void SortCurrentCsvBackToOriginalOrder()
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
            nextRowEdit.DoEdit(CurrentCsv!, this);
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
            currentRowEdit.UndoEdit(CurrentCsv!, this);
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

public class CompoundEdit : IReversibleEdit
{
    public bool MakeBusy { get; set; } = true;
    private readonly ICollection<IReversibleEdit> _edits;
    public CompoundEdit(ICollection<IReversibleEdit> edits)
    {
        _edits = edits;
    }

    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        foreach (var edit in _edits)
        {
            edit.DoEdit(csv, runner);
        }
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        foreach (var edit in _edits.Reverse())
        {
            edit.UndoEdit(csv, runner);
        }
    }
}

public class PerformMultipleCellEdits : IReversibleEdit
{
    public bool MakeBusy { get; set; } = false;
    private readonly ICollection<CellEdit> _cellEdits;
    private readonly string _columnName;

    public PerformMultipleCellEdits(ICollection<CellEdit> cellEdits, string columnName)
    {
        _cellEdits = cellEdits;
        _columnName = columnName;
    }

    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        foreach (var cellEdit in _cellEdits)
        {
            cellEdit.Row[_columnName] = cellEdit.AfterValue;
        }
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        foreach (var cellEdit in _cellEdits)
        {
            cellEdit.Row[_columnName] = cellEdit.BeforeValue;
        }
    }
}

public class AddRowsEdit : IReversibleEdit
{
    public bool MakeBusy { get; set; }
    private readonly HashSet<CsvRow> _rowsToAdd;
    public AddRowsEdit(ICollection<CsvRow> rowsToAdd)
    {
        _rowsToAdd = new HashSet<CsvRow>(rowsToAdd);
    }

    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        int requiredCapacity = csv.CsvContent.Count + _rowsToAdd.Count;
        if (csv.CsvContent.Capacity < requiredCapacity)
        {
            csv.CsvContent.Capacity = requiredCapacity;
        }
        foreach (var row in _rowsToAdd)
        {
            csv.CsvContent.Add(row);
        }
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        var remainingRows = new List<CsvRow>(csv.RowCount() - _rowsToAdd.Count);
        foreach (var row in csv.CsvContent)
        {
            if (!_rowsToAdd.Contains(row))
            {
                remainingRows.Add(row);
            }
        }
        csv.SetCsvContent(remainingRows);
    }
}

public class DeleteRowEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    private readonly CsvRow _row;
    public DeleteRowEdit(CsvRow row)
    {
        _row = row;
    }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.CsvContent.Remove(_row);
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.CsvContent.Add(_row);
    }
}

public class DeleteRowsEdit : IReversibleEdit
{
    public bool MakeBusy { get; set; } = false;
    private readonly HashSet<CsvRow> _rowsToDelete;

    public DeleteRowsEdit(ICollection<CsvRow> rowsToDelete)
    {
        _rowsToDelete = new HashSet<CsvRow>(rowsToDelete);
    }

    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        var remainingRows = new List<CsvRow>(csv.RowCount() - _rowsToDelete.Count);
        foreach (var row in csv.CsvContent)
        {
            if (!_rowsToDelete.Contains(row))
            {
                remainingRows.Add(row);
            }
        }
        csv.SetCsvContent(remainingRows);
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        int requiredCapacity = csv.CsvContent.Count + _rowsToDelete.Count;
        if (csv.CsvContent.Capacity < requiredCapacity)
        {
            csv.CsvContent.Capacity = requiredCapacity;
        }
        foreach (var row in _rowsToDelete)
        {
            csv.CsvContent.Add(row);
        }
    }
}

public class ModifyRowEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    public ModifyRowEdit(CsvRow row, CsvRow rowClone, CsvRow rowAfterOperation)
    {
        Row = row;
        RowClone = rowClone;
        RowAfterOperation = rowAfterOperation;
    }

    public CsvRow Row { get; }
    public CsvRow RowClone { get; }
    public CsvRow RowAfterOperation { get; }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
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
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
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

public class AddColumnEdit : IReversibleEdit
{
    public bool MakeBusy { get; set; } = true;
    private readonly string _columnName;
    private readonly string? _defaultValue;
    public AddColumnEdit(string columnName, string? defaultValue)
    {
        _columnName = columnName;
        _defaultValue = defaultValue;
    }

    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.AddColumn(_columnName, _defaultValue);
        runner.RecomputeCurrentColumnNames();
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.RemoveColumn(_columnName);
        runner.RecomputeCurrentColumnNames();
    }
}

internal struct RemoveColumnEditRow : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    private readonly Dictionary<string, object?> _newDictionary;
    private readonly CsvRow _row;
    private readonly string _columnName;
    private readonly string? _beforeValue;
    private readonly int _columnIndex;
    private string[] _columnNamesBefore;
    public RemoveColumnEditRow(CsvRow row, string columnName, string[] columnNamesBefore, Dictionary<string, object?> newDictionary)
    {
        _row = row;
        _columnName = columnName;
        _beforeValue = row[_columnName]?.ToString();
        _columnNamesBefore = columnNamesBefore;
        _newDictionary = newDictionary;
        _columnIndex = _columnNamesBefore.IndexOf(columnName);
    }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        _row.Remove(_columnName);
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        int i = 0;
        foreach (var kvp in _row)
        {
            if (i == _columnIndex)
            {
                _newDictionary[_columnName] = _beforeValue;
            }
            _newDictionary[kvp.Key] = kvp.Value;
            i++;
        }
        _row.Clear();
        foreach (var kvp in _newDictionary)
        {
            _row.Add(kvp.Key, kvp.Value);
        }
    }
}

public class SwapColumnEdit : IReversibleEdit
{
    private readonly string _columnName;
    private readonly string _otherColumnName;
    public bool MakeBusy { get; set; }

    public SwapColumnEdit(string columnName, string otherColumnName)
    {
        _columnName = columnName;
        _otherColumnName = otherColumnName;
    }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.Mutate(x =>
        {
            x.SwapColumns(_columnName, _otherColumnName);
        }, saveChanges: false);
        runner.RecomputeCurrentColumnNames();
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.Mutate(x =>
        {
            x.SwapColumns(_columnName, _otherColumnName);
        }, saveChanges: false);
        runner.RecomputeCurrentColumnNames();
    }
}

public class MoveColumnEdit : IReversibleEdit
{
    public MoveColumnEdit(string columnName, int newIndex)
    {
        ColumnName = columnName;
        NewIndex = newIndex;
    }
    private int? _oldIndex;
    public string ColumnName { get; }
    public int NewIndex { get; }
    public bool MakeBusy { get; set; } = true;
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        if (_oldIndex == null)
        {
            _oldIndex = csv.ColumnNames()!.IndexOf(ColumnName);
        }
        csv.MoveColumn(ColumnName, NewIndex);
        runner.RecomputeCurrentColumnNames();
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.MoveColumn(ColumnName, _oldIndex!.Value);
        runner.RecomputeCurrentColumnNames();
    }
}

public class RenameColumnEdit : IReversibleEdit
{
    public RenameColumnEdit(string columnName, string newColumnName)
    {
        ColumnName = columnName;
        NewColumnName = newColumnName;
    }
    public string ColumnName { get; }
    public string NewColumnName { get; }
    public bool MakeBusy { get; set; } = true;
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.ReplaceColumn(ColumnName, NewColumnName);
        runner.RecomputeCurrentColumnNames();
    }

    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        csv.ReplaceColumn(NewColumnName, ColumnName);
        runner.RecomputeCurrentColumnNames();
    }
}

public class RemoveColumnEdit : IReversibleEdit
{
    public bool MakeBusy { get; set; } = true;
    private readonly string _columnName;
    private string[]? _columnValuesBefore;
    private readonly Dictionary<string, object?> _newDictionary = new();
    private RemoveColumnEditRow[]? _removeColumnEdits;
    public RemoveColumnEdit(string columnName)
    {
        _columnName = columnName;
    }

    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        if (_columnValuesBefore == null)
        {
            _columnValuesBefore = csv.ColumnNames()!;
            _removeColumnEdits = csv.CsvContent.Select(x => new RemoveColumnEditRow(x, _columnName, _columnValuesBefore, _newDictionary)).ToArray();
        }
        foreach (var removeColumnEditRow in _removeColumnEdits!)
        {
            removeColumnEditRow.DoEdit(csv, runner);
        }
        runner.RecomputeCurrentColumnNames();
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        if (_removeColumnEdits != null)
        {
            foreach (var removeColumnEditRow in _removeColumnEdits)
            {
                removeColumnEditRow.UndoEdit(csv, runner);
            }
            runner.RecomputeCurrentColumnNames();
        }
    }
}

public class AddTagEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    public AddTagEdit(CsvRow row, string tagToAdd)
    {
        Row = row;
        TagToAdd = tagToAdd;
    }

    private bool _addedTag;
    public CsvRow Row { get; }
    public string TagToAdd { get; }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        if (!runner.CurrentTags!.Contains(TagToAdd))
        {
            runner.CurrentTags.Add(TagToAdd);
            _addedTag = true;
        }
        Row.AddProcessingTag(TagToAdd);
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.RemoveProcessingTag(TagToAdd);
        if (_addedTag)
        {
            runner.CurrentTags!.Remove(TagToAdd);
        }
    }
}

public class RemoveTagEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    public RemoveTagEdit(CsvRow row, string tagToRemove)
    {
        Row = row;
        TagToRemove = tagToRemove;
    }

    public CsvRow Row { get; }
    public string TagToRemove { get; }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.RemoveProcessingTag(TagToRemove);
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.AddProcessingTag(TagToRemove);
    }
}
public class RemoveReferenceEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    public RemoveReferenceEdit(CsvRow row, int referenceCsvId, int referenceRowId)
    {
        Row = row;
        ReferenceCsvId = referenceCsvId;
        ReferenceRowId = referenceRowId;
    }

    public CsvRow Row { get; }
    public int ReferenceCsvId { get; }
    public int ReferenceRowId { get; }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.RemoveProcessingReference(ReferenceCsvId, ReferenceRowId);
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.AddProcessingReference(ReferenceCsvId, ReferenceRowId);
    }
}
public class AddReferenceEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    public AddReferenceEdit(CsvRow row, int referenceCsvId, int referenceRowId)
    {
        Row = row;
        ReferenceCsvId = referenceCsvId;
        ReferenceRowId = referenceRowId;
    }

    public CsvRow Row { get; }
    public int ReferenceCsvId { get; }
    public int ReferenceRowId { get; }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.AddProcessingReference(ReferenceCsvId, ReferenceRowId);
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        Row.RemoveProcessingReference(ReferenceCsvId, ReferenceRowId);
    }
}

public interface IReversibleEdit
{
    public bool MakeBusy { get; }
    void DoEdit(IEasyCsv csv, StrategyRunner runner);
    void UndoEdit(IEasyCsv csv, StrategyRunner runner);
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
