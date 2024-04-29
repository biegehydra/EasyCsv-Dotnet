using Microsoft.AspNetCore.Components;

namespace EasyCsv.Components;
public class EasyCsvStrategyOptionsBase : ComponentBase
{
    [CascadingParameter] protected CsvProcessingStepper CsvProcessor { get; set; } = null!;
    [CascadingParameter] protected StrategyBucket StrategyBucket { get; set; } = null!;
}
