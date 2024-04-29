namespace EasyCsv.Components;

public class StrategyBucket
{
    public string ColumnName { get; }
    private readonly HashSet<StrategyItem> _strategies = new();
    public IReadOnlyCollection<StrategyItem> Strategies => _strategies;
    private Func<Task> ReRenderStepper { get; }
    internal StrategyBucket(string columnName, Func<Task> reRenderStepper)
    {
        ColumnName = columnName;
        ReRenderStepper = reRenderStepper;
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
}