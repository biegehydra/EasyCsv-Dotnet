using System;
using EasyCsv.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyCsv.Core.Extensions;

[assembly: InternalsVisibleTo("EasyCsv.Components")]
namespace EasyCsv.Processing;
public class StrategyRunner
{
    private int _currentIndex = -1;
    private int _currentEditIndex = -1;
    public int CurrentIndex => _currentIndex;
    public int? CurrentRowEditIndex => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].CurrentRowEditIndex.Value : null;
    public IEasyCsv? CurrentCsv => IsCacheIndexValid(_currentIndex) ? _steps[_currentIndex].Csv : null;
    public HashSet<string>? CurrentTags => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].Tags : null;
    internal List<IReversibleEdit>? CurrentReversibleEdits => Utils.IsValidIndex(_currentIndex, _steps.Count) ? _steps[_currentIndex].ReversibleEdits : null;
    private readonly List<(IEasyCsv Csv, string FileName)> _referenceCsvs = new();
    public IReadOnlyList<(IEasyCsv Csv, string FileName)> ReferenceCsvs => _referenceCsvs;
    public IReadOnlyList<IEasyCsv> CachedCsvs() => _steps.Select(x => x.Csv).ToArray();
    public IReadOnlyList<IReadOnlyCollection<string>> CachedTags() => _steps.Select(x => x.Tags).ToArray();
    internal readonly List<(IEasyCsv Csv, HashSet<string> Tags, List<IReversibleEdit> ReversibleEdits, CurrentRowEditIndex CurrentRowEditIndex)> _steps = new();

    public StrategyRunner(IEasyCsv baseCsv)
    {
        var clone = baseCsv.Clone();
        _steps.Add((clone, AllUniqueTags(clone), new List<IReversibleEdit>(), new CurrentRowEditIndex()));
        SetCurrentIndexSafe(0);
    }

    public void AddReversibleEdit(IReversibleEdit reversibleEdit)
    {
        if (CurrentCsv == null) return;
        while (_steps[_currentIndex].CurrentRowEditIndex.Value != _steps[_currentIndex].ReversibleEdits.Count - 1)
        {
            _steps[_currentIndex].ReversibleEdits.Remove(_steps[_currentIndex].ReversibleEdits[_steps[_currentIndex].ReversibleEdits.Count - 1]);
        }
        _steps[_currentIndex].ReversibleEdits.Add(reversibleEdit);
        GoForwardEdit();
    }

    public async ValueTask<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvColumnDeleteEvaluator was null");
        var clone = CurrentCsv.Clone();
        List<int> rowsToDelete = new List<int>();
        int i = -1;
        foreach (var (row, index) in clone.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            i++;
            var operationResult = await evaluateDelete.EvaluateDelete(new RowCell(row, evaluateDelete.ColumnName, index));
            if (operationResult.Delete)
            {
                rowsToDelete.Add(i);
            }
            if (operationResult.Success == false)
            {
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }
        await clone.MutateAsync(x => x.DeleteRows(rowsToDelete));
        string message = $"Deleted {rowsToDelete.Count} rows";

        AddToTimeline(clone);
        return new AggregateOperationDeleteResult(true, rowsToDelete.Count, message);
    }

    public async ValueTask<AggregateOperationDeleteResult> RunRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvRowDeleteEvaluator was null");

        var clone = CurrentCsv.Clone();
        List<int> rowsToDelete = new List<int>();
        int i = -1;
        foreach (var (row, index) in clone.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            i++;
            var operationResult = await evaluateDelete.EvaluateDelete(row, index);
            if (operationResult.Delete)
            {
                rowsToDelete.Add(i);
            }
            if (operationResult.Success == false)
            {
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }
        await clone.MutateAsync(x => x.DeleteRows(rowsToDelete));
        string message = $"Deleted {rowsToDelete.Count} rows";

        AddToTimeline(clone);
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
        var clone = CurrentCsv.Clone();
        foreach (var (row, index) in clone.CsvContent.FilterByIndexesWithOriginalIndex(filteredRowIds))
        {
            var operationResult = await columnProcessor.ProcessCell(new RowCell(row, columnProcessor.ColumnName, index));
            if (operationResult.Success == false)
            {
                return operationResult;
            }
        }

        AddToTimeline(clone);
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
        _steps.Add((csv, AllUniqueTags(csv), new List<IReversibleEdit>(), new CurrentRowEditIndex()));
        SetCurrentIndexSafe(_steps.Count - 1);
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
public class ModifyRowEdit(int rowIndex, CsvRow beforeRow, CsvRow aftereRow) : IReversibleEdit
{
    public int RowIndex { get; } = rowIndex;
    public CsvRow BeforeRow { get; } = beforeRow;
    public CsvRow AftereRow { get; } = aftereRow;
    public void DoEdit(IEasyCsv csv)
    {
#if DEBUG
        var equals = csv!.CsvContent[RowIndex].ValuesEqual(BeforeRow);
        if (!equals)
        {
            throw new Exception("Should've equalled Before Row");
        }
#endif
        csv!.CsvContent[RowIndex] = AftereRow;
    }
    public void UndoEdit(IEasyCsv csv)
    {
#if DEBUG
        var equals = csv!.CsvContent[RowIndex].ValuesEqual(AftereRow);
        if (!equals)
        {
            throw new Exception("Should've equalled After Row");
        }
#endif
        csv!.CsvContent[RowIndex] = BeforeRow;
    }
}
public class AddTagEdit : IReversibleEdit
{
    public AddTagEdit(int rowIndex, string tagToAdd, int tagColumnIndex = -1)
    {
        RowIndex = rowIndex;
        TagToAdd = tagToAdd;
        TagColumnIndex = tagColumnIndex;
    }
    public int RowIndex { get; }
    public string TagToAdd { get; }
    public int TagColumnIndex { get; }
    public void DoEdit(IEasyCsv csv)
    {
        csv!.CsvContent[RowIndex].AddProcessingTag(TagToAdd);
    }
    public void UndoEdit(IEasyCsv csv)
    {
        csv!.CsvContent[RowIndex].RemoveProcessingTag(TagToAdd);
    }
}
public class DeleteRowEdit : IReversibleEdit
{
    private readonly CsvRow _row;

    public DeleteRowEdit(int rowIndex, CsvRow row)
    {
        _row = row;
        RowIndex = rowIndex;
    }

    public int RowIndex { get; }
    public void DoEdit(IEasyCsv csv)
    {
        csv!.CsvContent.RemoveAt(RowIndex);
    }
    public void UndoEdit(IEasyCsv csv)
    {
        csv!.CsvContent.Insert(RowIndex, _row);
    }
}

public class RemoveTagEdit : IReversibleEdit
{
    public RemoveTagEdit(int rowIndex, string tagToRemove, int tagColumnIndex = -1)
    {
        RowIndex = rowIndex;
        TagToRemove = tagToRemove;
        TagColumnIndex = tagColumnIndex;
    }

    public int RowIndex { get; }
    public string TagToRemove { get; }
    public int TagColumnIndex { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (TagColumnIndex >= 0)
        {
            csv!.CsvContent[RowIndex].RemoveProcessingTag(TagToRemove);
        }
        else
        {
            csv!.CsvContent[RowIndex].RemoveProcessingTag(TagColumnIndex, TagToRemove);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (TagColumnIndex >= 0)
        {
            csv!.CsvContent[RowIndex].AddProcessingTag(TagToRemove);
        }
        else
        {
            csv!.CsvContent[RowIndex].AddProcessingTag(TagColumnIndex, TagToRemove);
        }
    }
}
public class RemoveReferenceEdit : IReversibleEdit
{
    public RemoveReferenceEdit(int rowIndex, int referenceCsvId, int referenceRowId, int referencesColumnIndex = -1)
    {
        RowIndex = rowIndex;
        ReferenceCsvId = referenceCsvId;
        ReferencesColumnIndex = referencesColumnIndex;
        ReferenceRowId = referenceRowId;
    }

    public int RowIndex { get; }
    public int ReferenceCsvId { get; }
    public int ReferencesColumnIndex { get; }
    public int ReferenceRowId { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            csv!.CsvContent[RowIndex].RemoveProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            csv!.CsvContent[RowIndex].RemoveProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            csv!.CsvContent[RowIndex].AddProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            csv!.CsvContent[RowIndex].AddProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
}
public class AddReferenceEdit : IReversibleEdit
{
    public AddReferenceEdit(int rowIndex, int referenceCsvId, int referenceRowId, int referencesColumnIndex = -1)
    {
        RowIndex = rowIndex;
        ReferenceCsvId = referenceCsvId;
        ReferencesColumnIndex = referencesColumnIndex;
        ReferenceRowId = referenceRowId;
    }

    public int RowIndex { get; }
    public int ReferenceCsvId { get; }
    public int ReferencesColumnIndex { get; }
    public int ReferenceRowId { get; }
    public void DoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            csv!.CsvContent[RowIndex].AddProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            csv!.CsvContent[RowIndex].AddProcessingReference(ReferenceCsvId, ReferenceRowId);
        }
    }
    public void UndoEdit(IEasyCsv csv)
    {
        if (ReferencesColumnIndex >= 0)
        {
            csv!.CsvContent[RowIndex].RemoveProcessingReference(ReferencesColumnIndex, ReferenceCsvId, ReferenceRowId);
        }
        else
        {
            csv!.CsvContent[RowIndex].RemoveProcessingReference(ReferenceCsvId, ReferenceRowId);
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

public readonly struct RowCell : ICell
{
    private CsvRow CsvRow { get; }

    public int RowIndex { get; }

    public string ColumnName { get; }

    public RowCell(CsvRow csvRow, string columnName, int rowIndex)
    {
        CsvRow = csvRow;
        ColumnName = columnName;
        RowIndex = rowIndex;
    }

    public object? Value
    {
        get
        {
            CsvRow.TryGetValue(ColumnName, out object? value);
            return value;
        }
        set => CsvRow[ColumnName] = value;
    }
}
