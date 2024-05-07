using EasyCsv.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyCsv.Core.Extensions;

[assembly: InternalsVisibleTo("EasyCsv.Components")]
namespace EasyCsv.Processing;
public class StrategyRunner
{
    private int _currentIndex = -1;
    public int CurrentIndex => _currentIndex;
    public IEasyCsv? CurrentCsv => IsCacheIndexValid(_currentIndex) ? _cachedCsvs[_currentIndex] : null;
    public HashSet<string>? CurrentTags => Utils.IsValidIndex(_currentIndex, _tagsCache.Count) ? _tagsCache[_currentIndex] : null;
    private readonly List<(IEasyCsv Csv, string FileName)> _referenceCsvs = new();
    public IReadOnlyList<(IEasyCsv Csv, string FileName)> ReferenceCsvs => _referenceCsvs;
    public IReadOnlyList<IEasyCsv> CachedCsvs => _cachedCsvs;
    private readonly List<IEasyCsv> _cachedCsvs = new();
    private List<HashSet<string>> _tagsCache = new();
    public IReadOnlyList<ICollection<string>> TagsCache => _tagsCache;

    public StrategyRunner(IEasyCsv baseCsv)
    {
        _cachedCsvs.Add(baseCsv.Clone());
        SetCurrentIndexSafe(0);
        _tagsCache.Add(AllUniqueTags());
    }

    public async ValueTask<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvColumnDeleteEvaluator was null");
        var clone = CurrentCsv.Clone();
        List<int> rowsToDelete = new List<int>();
        int i = -1;
        foreach (var row in clone.CsvContent.FilterByIndexes(filteredRowIds))
        {
            i++;
            var operationResult = await evaluateDelete.EvaluateDelete(new RowCell(row, evaluateDelete.ColumnName));
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
        foreach (var row in clone.CsvContent.FilterByIndexes(filteredRowIds))
        {
            i++;
            var operationResult = await evaluateDelete.EvaluateDelete(row);
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
        foreach (var row in clone.CsvContent.FilterByIndexes(filteredRowIds))
        {
            var operationResult = await columnProcessor.ProcessCell(new RowCell(row, columnProcessor.ColumnName));
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
        foreach (var row in clone.CsvContent.FilterByIndexes(filteredRowIds))
        {
            var operationResult = await rowProcessor.ProcessRow(row);
            if (operationResult.Success == false)
            {
                return operationResult;
            }
        }
        AddToTimeline(clone);
        return new OperationResult(true);
    }

    public async ValueTask<OperationResult> RunCsvStrategy(ICsvProcessor csvProcessor, ICollection<int>? filteredRowIds)
    {
        if (CurrentCsv == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentCsv.Clone();
        var operationResult = await csvProcessor.ProcessCsv(clone, filteredRowIds);
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
        while (_currentIndex != _cachedCsvs.Count - 1)
        {
#if NETSTANDARD2_0
            _cachedCsvs.Remove(_cachedCsvs[_cachedCsvs.Count - 1]);
            _tagsCache.Remove(_tagsCache[_tagsCache.Count - 1]);
#else
            _cachedCsvs.RemoveAt(_cachedCsvs.Count-1);
            _tagsCache.RemoveAt(_tagsCache.Count-1);
#endif
        }
        _cachedCsvs.Add(csv);
        SetCurrentIndexSafe(_cachedCsvs.Count - 1);
        _tagsCache.Add(AllUniqueTags());
    }

    internal HashSet<string> AllUniqueTags()
    {
        var uniqueTags = new HashSet<string>();
        if (CurrentCsv?.CsvContent == null) return uniqueTags;
        foreach (var row in CurrentCsv.CsvContent)
        {
            var tags = row.Tags();
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
        return Utils.IsValidIndex(index, _cachedCsvs.Count);
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

public readonly struct RowCell : ICell
{
    private CsvRow CsvRow { get; }
    public string ColumnName { get; }

    public RowCell(CsvRow csvRow, string columnName)
    {
        CsvRow = csvRow;
        ColumnName = columnName;
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
