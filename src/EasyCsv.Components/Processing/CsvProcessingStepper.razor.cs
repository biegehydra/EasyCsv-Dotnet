using EasyCsv.Components.Enums;
using EasyCsv.Core;
using EasyCsv.Processing;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;

namespace EasyCsv.Components;

public partial class CsvProcessingStepper
{
    [Inject] public ISnackbar? Snackbar { get; set; }
    [Parameter] public IEasyCsv? EasyCsv { get; set; }
    [Parameter] public bool UseSnackBar { get; set; } = true;

    /// <summary>
    /// If true, this component will make a clone of the provided
    /// EasyCsv and operate on the clone
    /// </summary>
    [Parameter] public RenderFragment<string>? ProcessingOptions { get; set; }
    [Parameter] public RenderFragment? ErrorBoundaryContent { get; set; }

    [Parameter] public CloseBehaviour CloseBehaviour { get; set; } = Enums.CloseBehaviour.CloseButtonAndClickAway;
    [Parameter] public bool HideOtherStrategiesOnSelect { get; set; } = true;
    [Parameter] public bool SearchBar { get; set; } = true;
    [Parameter] public bool ShowColumnNameInStrategySelect { get; set; } = true;
    [Parameter] public bool ShowAddReferenceCsvs { get; set; } = true;
    [Parameter] public RunOperationNoneSelectedBehaviour RunOperationNoneSelectedBehaviour { get; set; } = RunOperationNoneSelectedBehaviour.Hidden;
    [Parameter] public ColumnLocation TagsAndReferencesLocation { get; set; } = ColumnLocation.Beginning;
    [Parameter] public bool AllowControlTagsAndReferencesLocation { get; set; } = true;
    [Parameter] public Color ReferenceChipColor { get; set; } = Color.Primary;
    [Parameter] public string MaxStrategySelectHeight { get; set; } = "600px";

    private int _currentIndex = -1;
    internal IEasyCsv? CurrentState => IsCacheIndexValid(_currentIndex) ? _cachedStates[_currentIndex] : null;
    protected override void OnParametersSet()
    {
        if (EasyCsv != null && CurrentState == null)
        {
            _cachedStates.Add(EasyCsv.Clone());
            SetCurrentIndexSafe(0);
        }
    }

    internal readonly List<(IEasyCsv Csv, string FileName)> ReferenceCsvs = new();
    private readonly List<IEasyCsv> _cachedStates = new();

    public void AddReferenceCsv(CsvUploadedArgs csvFileArgs, bool useSnackbar = true)
    {
        if (csvFileArgs.Csv == null!) return;
        string fileName = !string.IsNullOrWhiteSpace(csvFileArgs.FileName)
            ? csvFileArgs.FileName
            : $"Unnamed Csv{ReferenceCsvs.Count + 1}";
        ReferenceCsvs.Add((csvFileArgs.Csv, fileName));
        if (useSnackbar)
        {
            Snackbar?.Add($"Added '{fileName}' to references");
        }
    }

    public async Task<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete)
    {
        if (CurrentState == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvColumnDeleteEvaluator was null");
        var clone = CurrentState.Clone();
        List<int> rowsToDelete = new List<int>();
        int i = -1;
        foreach (var row in clone.CsvContent)
        {
            i++;
            var operationResult = await evaluateDelete.EvaluateDelete(new RowCell(row, evaluateDelete.ColumnName));
            if (operationResult.Delete)
            {
                rowsToDelete.Add(i);
            }
            if (operationResult.Success == false)
            {
                if (UseSnackBar)
                {
                    Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
                }
                await InvokeAsync(StateHasChanged);
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }
        await clone.MutateAsync(x => x.DeleteRows(rowsToDelete));
        string message = $"Deleted {rowsToDelete.Count} rows";
        if (UseSnackBar)
        {
            Snackbar?.Add(message);
        }

        AddToTimeline(clone);
        await InvokeAsync(StateHasChanged);
        return new AggregateOperationDeleteResult(true, rowsToDelete.Count, message);
    }

    public async Task<AggregateOperationDeleteResult> PerformRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete)
    {
        if (CurrentState == null) return new AggregateOperationDeleteResult(false, 0, "Component not initialized yet.");
        if (evaluateDelete == null!) return new AggregateOperationDeleteResult(false, 0, "CsvRowDeleteEvaluator was null");

        var clone = CurrentState.Clone();
        List<int> rowsToDelete = new List<int>();
        int i = -1;
        foreach (var row in clone.CsvContent)
        {
            i++;
            var operationResult = await evaluateDelete.EvaluateDelete(row);
            if (operationResult.Delete)
            {
                rowsToDelete.Add(i);
            }
            if (operationResult.Success == false)
            {
                if (UseSnackBar)
                {
                    Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
                }
                await InvokeAsync(StateHasChanged);
                return new AggregateOperationDeleteResult(false, 0, operationResult.Message);
            }
        }
        await clone.MutateAsync(x => x.DeleteRows(rowsToDelete));
        string message = $"Deleted {rowsToDelete.Count} rows";
        if (UseSnackBar)
        {
            Snackbar?.Add(message);
        }

        AddToTimeline(clone);
        await InvokeAsync(StateHasChanged);
        return new AggregateOperationDeleteResult(true, rowsToDelete.Count, message);
    }

    public async Task<OperationResult> PerformReferenceStrategy(ICsvReferenceProcessor csvReferenceProcessor)
    {
        if (csvReferenceProcessor == null!) return new OperationResult(false, "CsvReferenceProcessor was null");
        int referenceId = csvReferenceProcessor.ReferenceCsvId;
        if (referenceId < 0 || referenceId >= ReferenceCsvs.Count)
        {
            return new OperationResult(false, "Invalid reference id");
        }
        if (CurrentState == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentState.Clone();
        var operationResult = await csvReferenceProcessor.ProcessCsv(clone, ReferenceCsvs[referenceId].Csv);
        if (operationResult.Success)
        {
            if (UseSnackBar && !string.IsNullOrWhiteSpace(operationResult.Message))
            {
                Snackbar?.Add(operationResult.Message, Severity.Success);
            }
            AddToTimeline(clone);
        }
        else if (UseSnackBar && !string.IsNullOrWhiteSpace(operationResult.Message))
        {
            Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
        }

        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformColumnStrategy(ICsvColumnProcessor columnProcessor)
    {
        if (CurrentState == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentState.Clone();
        foreach (var row in clone.CsvContent)
        {
            var operationResult = await columnProcessor.ProcessCell(new RowCell(row, columnProcessor.ColumnName));
            if (operationResult.Success == false)
            {
                if (UseSnackBar)
                {
                    Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
                }
                await InvokeAsync(StateHasChanged);
                return operationResult;
            }
        }

        AddToTimeline(clone);
        await InvokeAsync(StateHasChanged);
        return new OperationResult(true);
    }

    public async Task<OperationResult> PerformCsvStrategy(ICsvProcessor csvProcessor)
    {
        if (CurrentState == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentState.Clone();
        var operationResult = await csvProcessor.ProcessCsv(clone);
        if (operationResult.Success)
        {
            if (UseSnackBar && !string.IsNullOrWhiteSpace(operationResult.Message))
            {
                Snackbar?.Add(operationResult.Message, Severity.Success);
            }
            AddToTimeline(clone);
        }
        else if (UseSnackBar && !string.IsNullOrWhiteSpace(operationResult.Message))
        {
            Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
        }

        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    private void AddToTimeline(IEasyCsv csv)
    {
        // If the timeline looks like this,
        // and we want to add to the timeline
        // with the ^ representing _currentIndex
        // 1->2->3->4->5
        //       ^
        // Then we want to remove 4 and 5, like so
        // 1->2->3->6
        while (_currentIndex != _cachedStates.Count - 1)
        {
            _cachedStates.Remove(_cachedStates[^1]);
        }
        _cachedStates.Add(csv);
        SetCurrentIndexSafe(_cachedStates.Count - 1);
    }

    private void GoBackStep()
    {
       SetCurrentIndexSafe(_currentIndex -1);
    }

    private void GoForwardStep()
    {
        SetCurrentIndexSafe(_currentIndex + 1);
    }

    private void SetCurrentIndexSafe(int index)
    {
        if (IsCacheIndexValid(index))
        {
            _currentIndex = index;
        }
    }

    public bool IsCacheIndexValid([NotNullWhen(true)] int? index)
    {
        return Utils.IsValidIndex(index, _cachedStates.Count);
    }

    public bool IsReferenceIndexValid([NotNullWhen(true)] int? index)
    {
        return Utils.IsValidIndex(index, ReferenceCsvs.Count);
    }

    private readonly struct RowCell : ICell
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
}