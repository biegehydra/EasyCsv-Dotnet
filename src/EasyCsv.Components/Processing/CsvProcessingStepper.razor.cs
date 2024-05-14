using EasyCsv.Components.Enums;
using EasyCsv.Components.Internal;
using EasyCsv.Components.Processing;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;
using EasyCsv.Processing;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using MudBlazorFix;

namespace EasyCsv.Components;

public partial class CsvProcessingStepper
{
    private static AggregateOperationDeleteResult _runnerNullAggregateDelete = new AggregateOperationDeleteResult(false, 0, "Runner was null");
    private static AggregateOperationDeleteResult _processingTableNullAggregateDelete = new AggregateOperationDeleteResult(false, 0, "Processing table was null");
    private static OperationResult _runnerNull = new OperationResult(false, "Runner was null");
    private static OperationResult _processingTableNull = new OperationResult(false, "Processing table was null");
    private static Func<Exception, OperationResult> _createOperationResultError = x => new OperationResult(false, $"Error performing operation: {x}");
    private static Func<Exception, AggregateOperationDeleteResult> _createAggregateDeleteOperationResultError = x => new AggregateOperationDeleteResult(false, 0, $"Error performing operation: {x}");
    private static Func<Exception, OperationDeleteResult> _createDeleteOperationResultError = x => new OperationDeleteResult(false, false, $"Error performing operation: {x}");
    public bool Busy { get; private set; }

    [Inject] public ISnackbar? Snackbar { get; set; }
    [Inject] public IDialogService? DialogService { get; set; }
    [Inject] public IJSRuntime? Js { get; set; }
    [Parameter] public IEasyCsv? EasyCsv { get; set; }
    [Parameter] public string? EasyCsvFileName { get; set; }
    [Parameter] public bool UseSnackBar { get; set; } = true;

    /// <summary>
    /// If true, this component will make a clone of the provided
    /// EasyCsv and operate on the clone
    /// </summary>
    [Parameter] public RenderFragment<string>? ColumnStrategies { get; set; }
    [Parameter] public RenderFragment? FullCsvStrategies { get; set; }
    [Parameter] public RenderFragment? ErrorBoundaryContent { get; set; }
    [Parameter] public RenderFragment? AfterCurrentStep { get; set; }

    [Parameter] public CloseBehaviour CloseBehaviour { get; set; } = Enums.CloseBehaviour.CloseButtonAndClickAway;
    [Parameter] public bool HideOtherStrategiesOnSelect { get; set; } = true;
    [Parameter] public bool SearchBar { get; set; } = true;
    [Parameter] public bool EnableRowEditing { get; set; } = true;
    [Parameter] public bool EnableRowDeleting { get; set; } = true;
    [Parameter] public bool EnableSorting { get; set; } = true;
    [Parameter] public bool ShowColumnNameInStrategySelect { get; set; } = true;
    [Parameter] public bool ShowAddReferenceCsv { get; set; } = true;
    [Parameter] public bool EnableChangeEditColumnValueType { get; set; } = true;
    [Parameter] public RunOperationNoneSelectedVisibility RunOperationNoneSelectedVisibility { get; set; } = RunOperationNoneSelectedVisibility.Hidden;
    [Parameter] public ColumnLocation TagsAndReferencesLocation { get; set; } = ColumnLocation.Beginning;
    [Parameter] public ResolveDuplicatesAutoSelect ResolveDuplicatesAutoSelect { get; set; } = ResolveDuplicatesAutoSelect.None;
    [Parameter] public bool AllowControlTagsAndReferencesLocation { get; set; } = true;
    [Parameter] public bool UseSearchBar { get; set; } = true;
    /// <summary>
    /// 2 is a good value for Blazor-Server, 50 is a good value for Blazor-Wasm
    /// </summary>
    [Parameter] public int DelayAfterProgressMilliseconds { get; set; } = 2;
    [Parameter] public double SearchDebounceInterval { get; set; } = 250;
    [Parameter] public string? ViewFullCsvOperationsIcon { get; set; } = EasyCsvIcons.ColumnStrategies;
    [Parameter] public bool HideExpandUnselected { get; set; }
    [Parameter] public string? ViewColumnOperationsIcon { get; set; } = EasyCsvIcons.FullCsvStrategies;
    /// <summary>
    /// If true, operations will only run on filtered rows,
    /// otherwise they will run on every row
    /// </summary>
    [Parameter] public Color ReferenceChipColor { get; set; } = Color.Primary;
    [Parameter] public string MaxStrategySelectHeight { get; set; } = "600px";
    [Parameter] public string DefaultDownloadFileName { get; set; } = "WorkingCsvSnapshot";
    [Parameter] public bool AutoControlExpandOptionsOnSelect { get; set; } = true;
    [Parameter] public bool OpenDownloadWithAllColumnsSelected { get; set; } = true; 
    [Parameter] public bool AutoSelectAllColumnsToSearch { get; set; } = true; 
    [Parameter] public ColumnValueType DefaultColumnValueType { get; set; } = ColumnValueType.Text; 
    [Parameter] public bool UseProgressContext { get; set; } = true; 
    public StrategyRunner? Runner { get; private set; }
    internal CsvProcessingTable? CsvProcessingTable { get; private set; }
    private int _initialRowCount;

    protected override void OnParametersSet()
    {
        if (EasyCsv != null && Runner == null)
        {
            _initialRowCount = EasyCsv.RowCount();
            Runner = new StrategyRunner(EasyCsv);
        }
    }

    public ValueTask<OperationResult> AddReferenceCsv(CsvUploadedArgs csvFileArgs, bool useSnackbar = true)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNull);
        if (csvFileArgs.Csv == null!) return ValueTask.FromResult(_processingTableNull);
        string fileName = !string.IsNullOrWhiteSpace(csvFileArgs.FileName)
            ? csvFileArgs.FileName
            : $"Unnamed Csv{Runner.ReferenceCsvs.Count + 1}";
        return PerformOperationWithCatch(() =>
        {
            Runner.AddReference(csvFileArgs.Csv, fileName);
            return new ValueTask<OperationResult>(new OperationResult(true));
        }, _createOperationResultError);
    }

    public ValueTask<AggregateOperationDeleteResult> PerformColumnEvaluateDelete(ICsvColumnDeleteEvaluator evaluateDelete, OperationProgressContext? progressContext = null)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNullAggregateDelete);
        if (CsvProcessingTable == null) return ValueTask.FromResult(_processingTableNullAggregateDelete);
        if (!UseProgressContext) progressContext = null;
        if (DelayAfterProgressMilliseconds != 2 && progressContext?.DelayAfterProgressMilliseconds == 2) progressContext.DelayAfterProgressMilliseconds = DelayAfterProgressMilliseconds;
        Func<double, Task>? onProgressFunc = progressContext == null ? null : progressContext.ProgressChanged;
        var filteredRowIds = FilteredRowIds(evaluateDelete);
        return PerformOperationWithCatch(async () =>
        {
            var aggregateOperationDeleteResult = await Runner.PerformColumnEvaluateDelete(evaluateDelete, filteredRowIds, onProgressFunc);
            if (progressContext != null)
            {
                FinishProgressContext(aggregateOperationDeleteResult, progressContext);
            }
            else
            {
                AddAggregateDeleteOperationResultSnackbar(aggregateOperationDeleteResult);
            }

            await CsvProcessingTable.InvokeStateHasChanged();
            return aggregateOperationDeleteResult;
        }, _createAggregateDeleteOperationResultError);
    }

    public ValueTask<AggregateOperationDeleteResult> PerformRowEvaluateDelete(ICsvRowDeleteEvaluator evaluateDelete, OperationProgressContext? progressContext = null)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNullAggregateDelete);
        if (CsvProcessingTable == null) return ValueTask.FromResult(_processingTableNullAggregateDelete);
        if (!UseProgressContext) progressContext = null;
        if (DelayAfterProgressMilliseconds != 2 && progressContext?.DelayAfterProgressMilliseconds == 2) progressContext.DelayAfterProgressMilliseconds = DelayAfterProgressMilliseconds;
        Func<double, Task>? onProgressFunc = progressContext == null ? null : progressContext.ProgressChanged;
        var filteredRowIds = FilteredRowIds(evaluateDelete);
        return PerformOperationWithCatch(async () =>
        {
            var aggregateOperationDeleteResult = await Runner.RunRowEvaluateDelete(evaluateDelete, filteredRowIds, onProgressFunc);
            if (progressContext != null)
            {
                progressContext.CompletedText = aggregateOperationDeleteResult.Message;
                FinishProgressContext(aggregateOperationDeleteResult, progressContext);
            }
            else
            {
                AddAggregateDeleteOperationResultSnackbar(aggregateOperationDeleteResult);
            }
            await CsvProcessingTable.InvokeStateHasChanged();
            return aggregateOperationDeleteResult;
        }, _createAggregateDeleteOperationResultError);
    }

    public async ValueTask<OperationResult> PerformDedupe(IFindDupesOperation findDupesOperation, int[]? referenceCsvIds)
    {
        if (Runner?.CurrentCsv == null || DialogService == null) return _runnerNull;
        if (CsvProcessingTable == null) return _processingTableNull;
        _mustSelectRow = findDupesOperation.MustSelectRow;
        _multiSelect = findDupesOperation.MultiSelect;
        _columnName = findDupesOperation.ColumnName;
        _autoSelectRow = findDupesOperation.AutoSelectRow;
        _autoSelectRows = findDupesOperation.AutoSelectRows;
        try
        {
            Busy = true;
            _duplicateRowsResolveVisible = true;
            await InvokeStateHasChangeAndWait();
            List<CsvRow> rowsToDelete = new();
            var filteredRowIds = FilteredRowIds(findDupesOperation);
            var referenceCsvs = Runner.ReferenceCsvs.Select((x, i) => (x.Csv, i))
                .Where(x => referenceCsvIds?.Contains(x.i) == true).ToArray();
            int kept = 0;
            int deleted = 0;
            await foreach (var duplicateGroup in findDupesOperation.YieldReturnDupes(Runner.CurrentCsv,
                               filteredRowIds, referenceCsvs))
            {
                string value = findDupesOperation.DuplicateValuePresenter != null
                    ? findDupesOperation.DuplicateValuePresenter(duplicateGroup.DuplicateValue)
                    : duplicateGroup.DuplicateValue;
                _duplicateGroup = duplicateGroup;
                _duplicateValue = value;
                StateHasChanged();
                if (findDupesOperation.MultiSelect)
                {
                    _multiResolveDuplicateRowTaskSource = new TaskCompletionSource<MultiDuplicateRootPickerResult?>();
                    _singleResolveDuplicateRowTaskSource = null;
                    _duplicateGroup = duplicateGroup;
                    var resolvedRows = await _multiResolveDuplicateRowTaskSource.Task;
                    if (resolvedRows == null)
                    {
                        var failedOperationResult = new OperationResult(false, $"Resolve duplicate roots cancelled");
                        if (UseSnackBar)
                        {
                            Snackbar?.Add("Dedupe operation aborted");
                        }
                        return failedOperationResult;
                    }

                    foreach (var possibleDuplicate in duplicateGroup.Duplicates)
                    {
                        if (resolvedRows.RowsToKeep.All(x => x.RowToKeep != possibleDuplicate.Item2))
                        {
                            deleted++;
                            rowsToDelete.Add(possibleDuplicate.Item2);
                        }
                        else
                        {
                            kept++;
                        }
                    }
                }
                else
                {
                    _singleResolveDuplicateRowTaskSource = new TaskCompletionSource<DuplicateRootPickerResult?>();
                    _multiResolveDuplicateRowTaskSource = null;
                    var resolvedRows = await _singleResolveDuplicateRowTaskSource.Task;
                    if (resolvedRows == null)
                    {
                        if (UseSnackBar)
                        {
                            Snackbar?.Add("Dedupe operation aborted");
                        }

                        var failedOperationResult = new OperationResult(false, $"Resolve duplicate roots cancelled");
                        return failedOperationResult;
                    }

                    foreach (var duplicate in duplicateGroup.Duplicates)
                    {
                        if (duplicate.Item2 != resolvedRows.RowToKeep)
                        {
                            deleted++;
                            rowsToDelete.Add(duplicate.Item2);
                        }
                        else
                        {
                            kept++;
                        }
                    }
                }
            }

            if (deleted > 0)
            {
                var deleteRowsEdit = new DeleteRowsEdit(rowsToDelete);
                await AddReversibleEdit(deleteRowsEdit);
            }

            var operationResult = new OperationResult(true, $"Dedupe operation complete - {deleted + kept} rows evaluated | {deleted} rows deleted | {kept} rows kept");
            OnAfterOperation(operationResult);
            return operationResult;
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error during operation: {ex}");
        }
        finally
        {
            _duplicateRowsResolveVisible = false;
            Busy = false;
            await InvokeStateHasChangeAndWait();
        }
    }

    public ValueTask<OperationResult> AddReversibleEdit(IReversibleEdit reversibleEdit, bool makeBusy=false)
    {
        if (CsvProcessingTable == null || Runner == null) return ValueTask.FromResult(_runnerNull);
        return PerformOperationWithCatch(() =>
        {
            if (Runner.AddReversibleEdit(reversibleEdit))
            {
                CheckAddedCsvsAfterEdit();
                CsvProcessingTable.ApplyCurrentColumnSort();
            }
            return new ValueTask<OperationResult>(new OperationResult(true));
        }, _createOperationResultError, makeBusy);
    }

    public ValueTask<OperationResult> PerformReferenceStrategy(ICsvReferenceProcessor csvReferenceProcessor, OperationProgressContext? progressContext = null)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNull);
        if (CsvProcessingTable == null) return ValueTask.FromResult(_processingTableNull);
        if (!UseProgressContext) progressContext = null;
        if (DelayAfterProgressMilliseconds != 2 && progressContext?.DelayAfterProgressMilliseconds == 2) progressContext.DelayAfterProgressMilliseconds = DelayAfterProgressMilliseconds;
        var filteredRowIds = FilteredRowIds(csvReferenceProcessor);
        return PerformOperationWithCatch(async () =>
        {
            var operationResult = await Runner.RunReferenceStrategy(csvReferenceProcessor, filteredRowIds);
            FinishProgressContext(operationResult, progressContext);
            OnAfterOperation(operationResult, progressContext: progressContext);
            await InvokeAsync(StateHasChanged);
            return operationResult;
        }, _createOperationResultError);
    }

    public ValueTask<OperationResult> PerformColumnStrategy(ICsvColumnProcessor columnProcessor, OperationProgressContext? progressContext = null)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNull);
        if (CsvProcessingTable == null) return ValueTask.FromResult(_processingTableNull);
        if (!UseProgressContext) progressContext = null;
        if (DelayAfterProgressMilliseconds != 2 && progressContext?.DelayAfterProgressMilliseconds == 2) progressContext.DelayAfterProgressMilliseconds = DelayAfterProgressMilliseconds;
        Func<double, Task>? onProgressFunc = progressContext == null ? null : progressContext.ProgressChanged;
        var filteredRowIds = FilteredRowIds(columnProcessor);
        return PerformOperationWithCatch(async () =>
        {
            var operationResult = await Runner.RunColumnStrategy(columnProcessor, filteredRowIds, onProgressFunc);
            FinishProgressContext(operationResult, progressContext);
            OnAfterOperation(operationResult, skipSuccessSnackbar: true, progressContext);
            await InvokeAsync(StateHasChanged);
            return operationResult;
        }, _createOperationResultError);
    }

    public ValueTask<OperationResult> PerformRowStrategy(ICsvRowProcessor rowProcessor, OperationProgressContext? progressContext = null)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNull);
        if (CsvProcessingTable == null) return ValueTask.FromResult(_processingTableNull);
        if (!UseProgressContext) progressContext = null;
        if (DelayAfterProgressMilliseconds != 2 && progressContext?.DelayAfterProgressMilliseconds == 2) progressContext.DelayAfterProgressMilliseconds = DelayAfterProgressMilliseconds;
        Func<double, Task>? onProgressFunc = progressContext == null ? null : progressContext.ProgressChanged;
        var filteredRowIds = FilteredRowIds(rowProcessor);
        return PerformOperationWithCatch(async () =>
        {
            var operationResult = await Runner.RunRowStrategy(rowProcessor, filteredRowIds, onProgressFunc);
            FinishProgressContext(operationResult, progressContext);
            OnAfterOperation(operationResult, skipSuccessSnackbar: true, progressContext);
            await InvokeAsync(StateHasChanged);
            return operationResult;
        }, _createOperationResultError);
    }

    public ValueTask<OperationResult> PerformCsvStrategy(IFullCsvProcessor fullCsvProcessor, OperationProgressContext? progressContext = null)
    {
        if (Runner == null) return ValueTask.FromResult(_runnerNull);
        if (CsvProcessingTable == null) return ValueTask.FromResult(_processingTableNull);
        if (!UseProgressContext) progressContext = null;
        if (DelayAfterProgressMilliseconds != 2 && progressContext?.DelayAfterProgressMilliseconds == 2) progressContext.DelayAfterProgressMilliseconds = DelayAfterProgressMilliseconds;
        var filteredRowIds = FilteredRowIds(fullCsvProcessor);
        return PerformOperationWithCatch(async () =>
        {
            var operationResult = await Runner.RunCsvStrategy(fullCsvProcessor, filteredRowIds);
            FinishProgressContext(operationResult, progressContext);
            OnAfterOperation(operationResult, progressContext: progressContext);
            await InvokeAsync(StateHasChanged);
            return operationResult;
        }, _createOperationResultError);
    }

    private async ValueTask<TOperationResult> PerformOperationWithCatch<TOperationResult>(Func<ValueTask<TOperationResult>> operation, Func<Exception, TOperationResult> createErrorResult, bool makeBusy=true) where TOperationResult : IOperationResult
    {
        try
        {
            if (makeBusy)
            {
                Busy = true;
                await InvokeStateHasChangeAndWait();
            }
            return await operation();
        }
        catch (Exception ex)
        {
            return createErrorResult(ex);
        }
        finally
        {
            if (makeBusy)
            {
                Busy = false;
                await InvokeStateHasChangeAndWait();
            }
        }
    }

    private void FinishProgressContext<T>(T operationResult, OperationProgressContext? progressContext = null) where T : IOperationResult
    {
        if (progressContext != null)
        {
            progressContext.CompletedText = operationResult.Message;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            progressContext.Completed(operationResult.Success); // Fire and forget
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }

    private void OnAfterOperation<T>(T operationResult, bool skipSuccessSnackbar = false, OperationProgressContext? progressContext = null) where T : IOperationResult
    {
        if (progressContext == null)
        {
            AddOperationResultSnackbar(operationResult, skipSuccess: skipSuccessSnackbar);
        }
        CheckAddedCsvsAfterOperation(operationResult);
    }

    public void GoForwardEdit()
    {
        if (Runner?.GoForwardEdit() == true)
        {
            StateHasChanged();
        }
    }

    public void GoBackEdit()
    {
        if (Runner?.GoBackEdit() == true)
        {
            StateHasChanged();
        }
    }

    private HashSet<int>? FilteredRowIds(ICsvProcessor processor)
    {
        if (CsvProcessingTable == null) return null;
        if (!processor.OperatesOnlyOnFilteredRows) return null;
        HashSet<int>? filteredIndexes = null;
        if (CsvProcessingTable.IsFiltered())
        {
            filteredIndexes = CsvProcessingTable.FilteredRowIndexes();
        }
        return filteredIndexes;
    }

    private void AddOperationResultSnackbar<T>(T operationResult, bool skipSuccess = false) where T : IOperationResult
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

    private void AddAggregateDeleteOperationResultSnackbar(AggregateOperationDeleteResult operationResult, OperationProgressContext? progressContext = null)
    {
        if (progressContext == null && operationResult.Success == false && UseSnackBar)
        {
            Snackbar?.Add($"Error Performing Strategy: {operationResult.Message}", Severity.Warning);
        }
        else if (progressContext == null && UseSnackBar)
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
        var tagsColumnIndex = Runner?.CurrentCsvColumnNames?.IndexOf(x => x == InternalColumnNames.Tags) ?? -1;
        return Runner?.CurrentCsv?.CsvContent
            .Select((row, index) => (row, index))
            .Where(x => x.row.AnyColumnContainsValues(_searchColumns, _sq))
            .Where(x => tagsColumnIndex < 0 || x.row.MatchesIncludeTagsAndExcludeTags(tagsColumnIndex, _includeTags, _excludeTags))
            .Select(x => x.index)
            .ToHashSet() ?? new HashSet<int>();
    }

    private async Task InvokeStateHasChangeAndWait()
    {
        await InvokeAsync(StateHasChanged);
        await Task.Delay(1);
    }

    #region Editing

    /// <summary>Locks Inline Edit mode, if true.</summary>
    [Parameter]
    [Category("Editing")]
    public bool ReadOnly { get; set; }

    /// <summary>Button commit edit click event.</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnCommitEditClick { get; set; }

    /// <summary>Button cancel edit click event.</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnCancelEditClick { get; set; }

    /// <summary>
    /// Event is called before the item is modified in inline editing.
    /// </summary>
    [Parameter]
    public EventCallback<object> OnPreviewEditClick { get; set; }

    /// <summary>Tooltip for the CommitEdit Button.</summary>
    [Parameter]
    [Category("Editing")]
    public string? CommitEditTooltip { get; set; }

    /// <summary>Tooltip for the CancelEdit Button.</summary>
    [Parameter]
    [Category("Editing")]
    public string? CancelEditTooltip { get; set; }

    /// <summary>Sets the Icon of the CommitEdit Button.</summary>
    [Parameter]
    [Category("Editing")]
    public string CommitEditIcon { get; set; } = Icons.Material.Filled.Check;

    /// <summary>Sets the Icon of the CancelEdit Button.</summary>
    [Parameter]
    [Category("Editing")]
    public string CancelEditIcon { get; set; } = Icons.Material.Filled.Cancel;

    /// <summary>
    /// Define if Cancel button is present or not for inline editing.
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public bool CanCancelEdit { get; set; } = true;

    /// <summary>
    /// Set the positon of the CommitEdit and CancelEdit button, if <see cref="P:MudBlazor.MudTableBase.IsEditable" /> IsEditable is true. Defaults to the end of the row
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public TableApplyButtonPosition ApplyButtonPosition { get; set; } = TableApplyButtonPosition.StartAndEnd;

    /// <summary>
    /// Set the positon of the StartEdit button, if <see cref="P:MudBlazor.MudTableBase.IsEditable" /> IsEditable is true. Defaults to the end of the row
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public TableEditButtonPosition EditButtonPosition { get; set; } = TableEditButtonPosition.StartAndEnd;

    /// <summary>Defines how a table row edit will be triggered</summary>
    [Parameter]
    [Category("Editing")]
    public TableEditTrigger EditTrigger { get; set; } = TableEditTrigger.EditButton;

    /// <summary>
    /// Defines the edit button that will be rendered when EditTrigger.EditButton
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public RenderFragment<EditButtonContext>? EditButtonContent { get; set; }

    /// <summary>
    /// The method is called before the item is modified in inline editing.
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public Action<object>? RowEditPreview { get; set; }

    /// <summary>
    /// The method is called when the edition of the item has been committed in inline editing.
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public Action<object>? RowEditCommit { get; set; }

    /// <summary>
    /// The method is called when the edition of the item has been canceled in inline editing.
    /// </summary>
    [Parameter]
    [Category("Editing")]
    public Action<object>? RowEditCancel { get; set; }

    [Parameter] [Category("Editing")] public bool IsEditRowSwitchingBlocked { get; set; } = true;

    #endregion
}