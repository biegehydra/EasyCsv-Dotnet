using Microsoft.AspNetCore.Components;

namespace EasyCsv.Components;

public class StrategyBucketItemBase : ComponentBase
{
    [CascadingParameter] protected CsvProcessingStepper CsvProcessor { get; set; } = null!;
    [CascadingParameter] protected StrategyBucket StrategyBucket { get; set; } = null!;
}
public class StrategyItemBase : StrategyBucketItemBase
{
    [Parameter] public virtual string? DisplayName { get; set; } 
    [Parameter] public virtual string? DescriptionStr { get; set; }
    [Parameter] public virtual RenderFragment? Description { get; set; }
}
