using EasyCsv.Components.Enums;
using EasyCsv.Components.Internal;
using EasyCsv.Components.Processing;
using EasyCsv.Components.Processing.Dialogs;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;
using EasyCsv.Processing;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.JSInterop;

namespace EasyCsv.Components;

public partial class CsvProcessingStepper
{
    private static AggregateOperationDeleteResult _runnerNullAggregateDelete = new AggregateOperationDeleteResult(false, 0, "Runner was null");
    private static AggregateOperationDeleteResult _processingTableNullAggregateDelete = new AggregateOperationDeleteResult(false, 0, "Processing table was null");
    private static OperationResult _runnerNull = new OperationResult(false, "Runner was null");
    private static OperationResult _processingTableNull = new OperationResult(false, "Processing table was null");
    public bool Busy { get; private set; }

    [Inject] public ISnackbar? Snackbar { get; set; }
    [Inject] public IDialogService? DialogService { get; set; }
    [Inject] public IJSRuntime? Js { get; set; }
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
    /// <summary>
    /// If true, operations will only run on filtered rows,
    /// otherwise they will run on every row
    /// </summary>
    [Parameter] public bool OperateOnFilteredRows { get; set; }
    [Parameter] public Color ReferenceChipColor { get; set; } = Color.Primary;
    [Parameter] public string MaxStrategySelectHeight { get; set; } = "600px";
    [Parameter] public string DefaultDownloadFileName { get; set; } = "CsvSnapshot";
    [Parameter] public bool AutoControlExpandOptionsOnSelect { get; set; } = true;
    [Parameter] public bool ShowAddCsv { get; set; } 
    [Parameter] public ExpectedHeaderConfig ExpectedHeaderConfig { get; set; } = new (DefaultValueType.Text, false, null, AutoMatching.Lenient);
    public StrategyRunner? Runner { get; private set; }
    private CsvProcessingTable? _csvProcessingTable { get; set; }

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

    public async Task<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete)
    {
        if (Runner == null) return _runnerNullAggregateDelete;
        if (_csvProcessingTable == null) return _processingTableNullAggregateDelete;
        var filteredRowIds = FilteredRowIds();
        var aggregateOperationDeleteResult = await Runner.PerformColumnEvaluateDelete(evaluateDelete, filteredRowIds);
        AddAggregateDeleteOperationResultSnackbar(aggregateOperationDeleteResult);
        await InvokeAsync(StateHasChanged);
        return aggregateOperationDeleteResult;
    }

    public async Task<AggregateOperationDeleteResult> PerformRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete)
    {
        if (Runner == null) return _runnerNullAggregateDelete;
        if (_csvProcessingTable == null) return _processingTableNullAggregateDelete;
        var filteredRowIds = FilteredRowIds();
        var aggregateOperationDeleteResult = await Runner.RunRowEvaluateDelete(evaluateDelete, filteredRowIds);
        AddAggregateDeleteOperationResultSnackbar(aggregateOperationDeleteResult);
        await InvokeAsync(StateHasChanged);
        return aggregateOperationDeleteResult;
    }

    public async Task<OperationResult> PerformDedupe(IFindDedupesOperation findDedupesOperation, int[]? referenceCsvIds)
    {
        if (Runner?.CurrentCsv == null || DialogService == null) return _runnerNull;
        if (_csvProcessingTable == null) return _processingTableNull;
        var filteredRowIds = FilteredRowIds();
        var referenceCsvs = Runner.ReferenceCsvs.Select((x, i) => (x.Csv, i)).Where(x => referenceCsvIds?.Contains(x.i) == true).ToArray();
        int kept = 0;
        int deleted = 0;
        var clone = Runner.CurrentCsv.Clone();
        await foreach (var duplicateGroup in findDedupesOperation.YieldReturnDupes(Runner.CurrentCsv, filteredRowIds, referenceCsvs))
        {
            string value = findDedupesOperation.DuplicateValuePresenter != null
                ? findDedupesOperation.DuplicateValuePresenter(duplicateGroup.DuplicateValue)
                : duplicateGroup.DuplicateValue;
            if (findDedupesOperation.MultiSelect)
            {
                var dialogParameters = new DialogParameters<MultiDuplicateRootPicker>()
                {
                    {x => x.Stepper, this},
                    {x => x.DuplicateGroup, duplicateGroup},
                    {x => x.MustSelectRow, findDedupesOperation.MustSelectRow},
                    {x => x.TitleText, findDedupesOperation.TitleText},
                    {x => x.DuplicateValue, value}
                };
                var dialog = await DialogService.ShowAsync<MultiDuplicateRootPicker>(null, dialogParameters, new DialogOptions()
                {
                    CloseButton = false,
                    CloseOnEscapeKey = false,
                    DisableBackdropClick = true
                });
                var dialogResult = await dialog.Result;
                if (dialogResult.Canceled)
                {
                    var failedOperationResult = new OperationResult(false, $"Resolve duplicate roots cancelled");
                    if (UseSnackBar)
                    {
                        Snackbar?.Add("Dedupe operation aborted");
                    }
                    await InvokeAsync(StateHasChanged);
                    return failedOperationResult;
                }
                if (dialogResult?.Data is MultiDuplicateRootPickerResult rootPickerResult)
                {
                    if (rootPickerResult.RowsToKeep.Any(y => y.ReferenceCsvId.HasValue)) throw new Exception();
                    var possibleRowsToDelete = duplicateGroup.Duplicates.First(x => x.Item1 == null).Item2;
                    foreach (var possibleDuplicate in possibleRowsToDelete)
                    {
                        if (rootPickerResult.RowsToKeep.All(x => x.RowToKeep != possibleDuplicate.Item2))
                        {
                            deleted++;
                            await clone.MutateAsync(x => x.DeleteRow(possibleDuplicate.Item2), false);
                        }
                        else
                        {
                            kept++;
                        }
                    }
                }
            }
            else
            {
                var dialogParameters = new DialogParameters<DuplicateRootPicker>()
                {
                    {x => x.Stepper, this},
                    {x => x.DuplicateGroup, duplicateGroup},
                    {x => x.MustSelectRow, findDedupesOperation.MustSelectRow},
                    {x => x.TitleText, findDedupesOperation.TitleText},
                    {x => x.DuplicateValue, value}
                };
                var dialog = await DialogService.ShowAsync<DuplicateRootPicker>(null, dialogParameters, new DialogOptions()
                {
                    CloseButton = false,
                    CloseOnEscapeKey = false,
                    DisableBackdropClick = true
                });
                var dialogResult = await dialog.Result;
                if (dialogResult.Canceled)
                {
                    if (UseSnackBar)
                    {
                        Snackbar?.Add("Dedupe operation aborted");
                    }
                    var failedOperationResult = new OperationResult(false, $"Resolve duplicate roots cancelled");
                    await InvokeAsync(StateHasChanged);
                    return failedOperationResult;
                }
                if (dialogResult?.Data is DuplicateRootPickerResult rootPickerResult)
                {
                    if (rootPickerResult.ReferenceCsvId.HasValue) throw new Exception();
                    var possibleRowsToDelete = duplicateGroup.Duplicates.First(x => x.Item1 == null).Item2;
                    foreach (var duplicate in possibleRowsToDelete)
                    {
                        if (duplicate.Item2 != rootPickerResult.RowToKeep)
                        {
                            deleted++;
                            await clone.MutateAsync(x => x.DeleteRow(duplicate.Item2), false);
                        }
                        else
                        {
                            kept++;
                        }
                    }
                }
            }
        }
        if (deleted > 0)
        {
            await clone.CalculateContentBytesAndStrAsync();
            Runner.AddToTimeline(clone);
        }

        var operationResult = new OperationResult(true, $"Dedupe operation complete - {deleted} rows deleted | {kept} rows kept");
        AddOperationResultSnackbar(operationResult);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformReferenceStrategy(ICsvReferenceProcessor csvReferenceProcessor)
    {
        if (Runner == null) return _runnerNull;
        if (_csvProcessingTable == null) return _processingTableNull;
        var filteredRowIds = FilteredRowIds();
        var operationResult = await Runner.RunReferenceStrategy(csvReferenceProcessor, filteredRowIds);
        AddOperationResultSnackbar(operationResult);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformColumnStrategy(ICsvColumnProcessor columnProcessor)
    {
        if (Runner == null) return _runnerNull;
        if (_csvProcessingTable == null) return _processingTableNull;
        var filteredRowIds = FilteredRowIds();
        var operationResult = await Runner.RunColumnStrategy(columnProcessor, filteredRowIds);
        AddOperationResultSnackbar(operationResult, skipSuccess: true);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformRowStrategy(ICsvRowProcessor rowProcessor)
    {
        if (Runner == null) return _runnerNull;
        if (_csvProcessingTable == null) return _processingTableNull;
        var filteredRowIds = FilteredRowIds();
        var operationResult = await Runner.RunRowStrategy(rowProcessor, filteredRowIds);
        AddOperationResultSnackbar(operationResult, skipSuccess: true);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    public async Task<OperationResult> PerformCsvStrategy(ICsvProcessor csvProcessor)
    {
        if (Runner == null) return _runnerNull;
        if (_csvProcessingTable == null) return _processingTableNull;
        var filteredRowIds = FilteredRowIds();
        var operationResult = await Runner.RunCsvStrategy(csvProcessor, filteredRowIds);
        AddOperationResultSnackbar(operationResult);
        await InvokeAsync(StateHasChanged);
        return operationResult;
    }

    private HashSet<int>? FilteredRowIds()
    {
        if (_csvProcessingTable == null) return null;
        HashSet<int>? filteredIndexes = null;
        if (_csvProcessingTable.IsFiltered())
        {
            filteredIndexes = _csvProcessingTable.FilteredRowIndexes();
        }
        return filteredIndexes;
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

    private async ValueTask DownloadSnapShot()
    {
        if (Js == null || Runner?.CurrentCsv == null || string.IsNullOrWhiteSpace(_fileName)) return;
        var rowIndexes = GetFilteredRowIndexesForDownload();
        if (rowIndexes.Count == 0)
        {
            _isDownloadDialogVisible = false;
            if (UseSnackBar)
            {
                Snackbar?.Add($"No rows matched filter, nothing to download.", Severity.Warning);
            }
            return;
        }
        if (!rowIndexes.Any()) return;
        var clone = Runner.CurrentCsv.Clone();
        var downloadModel = clone.CondenseTo(_columnsToDownload, rowIndexes);
        if (downloadModel?.ContentBytes == null) return;
        _isDownloadDialogVisible = false;
        using var streamRef = new DotNetStreamReference(stream:
            new MemoryStream(downloadModel.ContentBytes));
        if (!_fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            _fileName += ".csv";
        }
        await Js.InvokeVoidAsync("downloadFileFromStreamEasyCsv", _fileName, streamRef);
        if (UseSnackBar)
        {
            Snackbar?.Add($"Downloaded {downloadModel.CsvContent.Count} rows");
        }
    }

    private HashSet<int> GetFilteredRowIndexesForDownload()
    {
        var tagsColumnIndex = Runner?.CurrentCsv?.ColumnNames()?.IndexOf(x => x == InternalColumnNames.Tags) ?? -1;
        return Runner?.CurrentCsv?.CsvContent
            .Select((row, index) => (row, index))
            .Where(x => x.row.AnyColumnContainsValues(_searchColumns, _sq))
            .Where(x => tagsColumnIndex < 0 || x.row.MatchesIncludeTagsAndExcludeTags(tagsColumnIndex, _includeTags, _excludeTags))
            .Select(x => x.index)
            .ToHashSet() ?? new HashSet<int>();
    }
}