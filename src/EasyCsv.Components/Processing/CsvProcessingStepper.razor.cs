using EasyCsv.Components.Enums;
using EasyCsv.Core;
using EasyCsv.Processing;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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
    [Parameter] public bool UseToolbar { get; set; } = true;
    [Parameter] public double SearchDebounceInterval { get; set; } = 250;
    [Parameter] public Color ReferenceChipColor { get; set; } = Color.Primary;
    [Parameter] public string MaxStrategySelectHeight { get; set; } = "600px";
    public StrategyRunner? Runner { get; private set; }
    protected override void OnParametersSet()
    {
        if (EasyCsv != null && Runner == null)
        {
            Runner = new StrategyRunner(EasyCsv);
        }
    }

    public void AddReferenceCsv(CsvUploadedArgs csvFileArgs, bool useSnackbar = true)
    {
        if (Runner == null) return;
        if (csvFileArgs.Csv == null!) return;
        string fileName = !string.IsNullOrWhiteSpace(csvFileArgs.FileName)
            ? csvFileArgs.FileName
            : $"Unnamed Csv{Runner.ReferenceCsvs.Count + 1}";
        Runner.AddReference(csvFileArgs.Csv, fileName);
        if (useSnackbar)
        {
            Snackbar?.Add($"Added '{fileName}' to references");
        }
    }
    private static AggregateOperationDeleteResult _runnerNullAggregateDelete = new AggregateOperationDeleteResult(false, 0, "Runner was null");
    private static OperationResult _runnerNull = new OperationResult(false, "Runner was null");
    public async Task<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete)
    {
        if (Runner == null) return _runnerNullAggregateDelete;
        var aggregateOperationDeleteResult = await Runner.PerformColumnEvaluateDelete(evaluateDelete);
        AddAggregateDeleteOperationResultSnackbar(aggregateOperationDeleteResult);
        await InvokeAsync(StateHasChanged);
        return aggregateOperationDeleteResult;
    }

    public async Task<AggregateOperationDeleteResult> PerformRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete)
    {
        if (Runner == null) return _runnerNullAggregateDelete;
        var aggregateOperationDeleteResult = await Runner.RunRowEvaluateDelete(evaluateDelete);
        AddAggregateDeleteOperationResultSnackbar(aggregateOperationDeleteResult);
        await InvokeAsync(StateHasChanged);
        return aggregateOperationDeleteResult;
    }

    public async Task<OperationResult> PerformReferenceStrategy(ICsvReferenceProcessor csvReferenceProcessor)
    {
        if (Runner == null) return _runnerNull;
        var operationResult = await Runner.RunReferenceStrategy(csvReferenceProcessor);
        AddOperationResultSnackbar(operationResult);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformColumnStrategy(ICsvColumnProcessor columnProcessor)
    {
        if (Runner == null) return _runnerNull;
        var operationResult = await Runner.RunColumnStrategy(columnProcessor);
        AddOperationResultSnackbar(operationResult, skipSuccess: true);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformCsvStrategy(ICsvProcessor csvProcessor)
    {
        if (Runner == null) return _runnerNull;
        var operationResult = await Runner.RunCsvStrategy(csvProcessor);
        AddOperationResultSnackbar(operationResult);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    private void AddOperationResultSnackbar(OperationResult operationResult, bool skipSuccess = false)
    {
        if (operationResult.Success && UseSnackBar && !string.IsNullOrWhiteSpace(operationResult.Message) && !skipSuccess)
        {
            Snackbar?.Add(operationResult.Message, Severity.Success);
        }
        else if (UseSnackBar && !string.IsNullOrWhiteSpace(operationResult.Message))
        {
            Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
        }
    }

    private void AddAggregateDeleteOperationResultSnackbar(AggregateOperationDeleteResult operationResult)
    {
        if (operationResult.Success == false && UseSnackBar)
        {
            Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
        }
        else if (UseSnackBar)
        {
            string message = $"Deleted {operationResult.Deleted} rows";
            Snackbar?.Add(message);

        }
    }

    public bool GoBackStep()
    {
        return Runner?.GoBackStep() ?? false;
    }

    public bool GoForwardStep()
    {
        return Runner?.GoForwardStep() ?? false;
    }
}