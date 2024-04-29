using EasyCsv.Core;
using EasyCsv.Processing;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EasyCsv.Components;

public partial class CsvProcessingStepper
{
    [Parameter] public IEasyCsv? EasyCsv { get; set; }
    [Parameter] public ISnackbar? Snackbar { get; set; }
    [Parameter] public bool UseSnackBars { get; set; } = true;

    /// <summary>
    /// If true, this component will make a clone of the provided
    /// EasyCsv and operate on the clone
    /// </summary>
    [Parameter]
    public RenderFragment<string>? ProcessingOptions { get; set; }

    private int _currentIndex = -1;
    internal IEasyCsv? CurrentState => IsIndexValid(_currentIndex) ? _cachedStates[_currentIndex] : null;
    protected override void OnParametersSet()
    {
        if (EasyCsv != null && CurrentState == null)
        {
            _cachedStates.Add(EasyCsv.Clone());
            SetCurrentIndexSafe(0);
        }
    }

    private readonly List<IEasyCsv> _cachedStates = new();

    public async Task<OperationResult> PerformColumnStrategy(ICsvColumnProcessor columnProcessor)
    {
        if (CurrentState == null) return new OperationResult(false, "Component not initialized yet.");
        var clone = CurrentState.Clone();
        foreach (var row in clone.CsvContent)
        {
            var operationResult = await columnProcessor.ProcessCell(new RowCell(row, columnProcessor.ColumnName));
            if (operationResult.Success == false)
            {
                if (UseSnackBars)
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
            if (UseSnackBars && !string.IsNullOrWhiteSpace(operationResult.Message))
            {
                Snackbar?.Add(operationResult.Message, Severity.Success);
            }
            AddToTimeline(clone);
        }
        else if (UseSnackBars && !string.IsNullOrWhiteSpace(operationResult.Message))
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
        if (IsIndexValid(index))
        {
            _currentIndex = index;
        }
    }

    private bool IsIndexValid(int index)
    {
        return index > 0 && index < _cachedStates.Count;
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