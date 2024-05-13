using MudBlazor;

namespace EasyCsv.Components.Processing;
public static class ProgressPopupExtensions
{
    public static Task OpenProgressSnackbar(this ISnackbar? snackbar, OperationProgressContext context, bool indeterminate, int deleteDelayMilliseconds = 5000, string positionClass = Defaults.Classes.Position.BottomRight, Action<SnackbarOptions>? configure = null)
    {
        if (snackbar == null) return Task.CompletedTask;
        if (configure == null)
        {
            configure = config =>
            {
                config.ActionVariant = Variant.Text;
                config.CloseAfterNavigation = false;
                config.HideIcon = true;
                config.ShowCloseIcon = false;
                config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;
                config.VisibleStateDuration = 1000000;
            };
        }
        var progressSnackbar = snackbar.Add<ProgressSnackbar>(new Dictionary<string, object>()
        {
            {"OperationProgressContext", context },
            {"Indeterminate", indeterminate},
            {"PositionClass", positionClass}, 
            {"DeleteDelayMilliseconds", deleteDelayMilliseconds}
        }, Severity.Normal, configure);
        context.Data = progressSnackbar;
        return Task.Delay(50);
    }
}