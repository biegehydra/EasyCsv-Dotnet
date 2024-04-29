using MudBlazor;

namespace EasyCsv.Components;

public class StrategyBucket
{
    public Origin TransformOrigin { get; private set; }
    public Origin AnchorOrigin { get; private set; }
    public string ColumnName { get; }
    public string SearchQuery { get; private set; }
    private readonly HashSet<StrategyItem> _strategies = new();
    public IReadOnlyCollection<StrategyItem> Strategies => _strategies;
    private Func<Task> ReRenderStepper { get; }
    internal StrategyBucket(string columnName, Func<Task> reRenderStepper)
    {
        ColumnName = columnName;
        ReRenderStepper = reRenderStepper;
    }

    public async Task SetSearchQuery(string query)
    {
        if (!query.Equals(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            SearchQuery = query;
            await ReRenderStepper();
            foreach (var strategy in _strategies)
            {
                await strategy.InvokeStateHasChanged();
            }
        }
    }

    public IEnumerable<StrategyItem> Search(string query)
    {
        return _strategies.Where(x => x.DisplayName?.Contains(query) == true || x.Description?.Contains(query) == true);
    }

    public async Task OnAllowRunChanged()
    {
        await ReRenderStepper();
    }

    public async Task OnSelectedChanged(StrategyItem strategyItem)
    {
        foreach (var strategy in _strategies.Where(x => x != strategyItem))
        {
            await strategy.InvokeStateHasChanged();
        }
        await ReRenderStepper();
    }

    public void Add(StrategyItem strategy)
    {
        if (strategy != null!)
        {
            _strategies.Add(strategy);
        }
    }

    public void Remove(StrategyItem strategy)
    {
        if (strategy != null!)
        {
            _strategies.Remove(strategy);
        }
    }

    internal bool IsOtherStrategySelected(StrategyItem item)
    {
        return _strategies.Any(x => x.IsSelected && x != item);
    }

    public bool IsAnySelected()
    {
        return _strategies.Any(x => x.IsSelected);
    }

    internal bool StrategySelectedAndRunAllowed()
    {
        if (!IsAnySelected()) return false;
        var strategy = _strategies.First(x => x.IsSelected);
        return strategy.AllowRun;
    }

    internal async Task StrategyPicked()
    {
        var strategy = _strategies.FirstOrDefault(x => x.IsSelected);
        if (strategy == null) return;
        await strategy.StrategyPicked.InvokeAsync(ColumnName);
    }

    internal bool MatchesSearchQuery(StrategyItem strategyItem)
    {
        return string.IsNullOrWhiteSpace(SearchQuery) || strategyItem.DisplayName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
               strategyItem.Description?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true;
    }

    internal void SetOrigins(Origin anchorOrigin, Origin transformOrigin)
    {
        AnchorOrigin = anchorOrigin;
        TransformOrigin = transformOrigin;
    }
}