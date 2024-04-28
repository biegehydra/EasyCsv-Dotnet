namespace EasyCsv.Components;

public class ColumnStrategyStepperConfig
{
    public string DisplayName { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, string>[] BeforeRows { get; }
    public IReadOnlyDictionary<string, string>[] AfterRows { get; }

    public ColumnStrategyStepperConfig(string strategyDisplayName, string description, IEnumerable<IReadOnlyDictionary<string, string>> beforeRows, IEnumerable<IReadOnlyDictionary<string, string>> afterRows)
    {
        DisplayName = strategyDisplayName;
        Description = description;
        BeforeRows = beforeRows.ToArray();
        AfterRows = afterRows.ToArray();
    }
}