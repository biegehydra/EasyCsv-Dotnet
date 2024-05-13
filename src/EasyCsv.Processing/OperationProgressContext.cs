using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyCsv.Components.Processing;
public class OperationProgressContext
{
    private readonly double _minProgressBetweenUpdates;
    public string Text { get; set; }
    public string? CompletedText { get; set; }
    public object? Data { get; set; }
    public event Func<string, Task>? OnStageChange;
    public event Func<bool, Task>? OnIndeterminateChanged;
    public event Func<double, Task>? OnProgressChange;
    public event Func<bool, Task>? OnCompleted;
    public event Func<Task>? OnAborted;
    private double _lastProgress;
    public OperationProgressContext(string text, double minProgressBetweenUpdates=0.005)
    {
        _minProgressBetweenUpdates = minProgressBetweenUpdates;
        Text = text;
    }

    public async Task ProgressChanged(double progress)
    {
        if (progress - _lastProgress < _minProgressBetweenUpdates) return;
        _lastProgress = progress;
        var handlers = OnProgressChange;
        if (handlers != null)
        {
            var tasks = handlers.GetInvocationList()
                .Cast<Func<double, Task>>()
                .Select(handler => handler(progress))
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }

    public async Task StageChanged(string stage)
    {
        var handlers = OnStageChange;
        if (handlers != null)
        {
            var tasks = handlers.GetInvocationList()
                .Cast<Func<string, Task>>()
                .Select(handler => handler(stage))
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }

    public async Task IndeterminateChanged(bool indeterminate)
    {
        var handlers = OnIndeterminateChanged;
        if (handlers != null)
        {
            var tasks = handlers.GetInvocationList()
                .Cast<Func<bool, Task>>()
                .Select(handler => handler(indeterminate))
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }

    public async Task Completed(bool success)
    {
        var handlers = OnCompleted;
        if (handlers != null)
        {
            var tasks = handlers.GetInvocationList()
                .Cast<Func<bool, Task>>()
                .Select(handler => handler(success))
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }

    public async Task Abort()
    {
        var handlers = OnAborted;
        if (handlers != null)
        {
            var tasks = handlers.GetInvocationList()
                .Cast<Func<Task>>()
                .Select(handler => handler())
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }
}