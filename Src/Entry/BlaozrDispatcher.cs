using Microsoft.AspNetCore.Components;

namespace BlazorApp.OpenHarmony;

public class BlaozrDispatcher : Dispatcher
{
    SingleThreadSyncContext _context;
    public BlaozrDispatcher()
    {
        _context = SingleThreadSyncContext.Initialize();
    }
    public override bool CheckAccess() => SynchronizationContext.Current == _context;

    public override Task InvokeAsync(Action workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem, "workItem");
        if (CheckAccess())
        {
            workItem();
            return Task.CompletedTask;
        }
        var tcs = new TaskCompletionSource();

         _context.Post(obj => {
             try
             {
                 workItem();
                 tcs.SetResult();
             }
             catch (Exception ex)
             {
                 tcs.SetException(ex);
             }
         }, null);

        return tcs.Task;
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem, "workItem");
        if (CheckAccess())
        {
            return workItem();
        }

        var tcs = new TaskCompletionSource();

        _context.Post(obj =>
        {
            try
            {
                workItem();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null); 

        return tcs.Task;
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem, "workItem");
        if (CheckAccess())
        {
            return Task.FromResult(workItem());
        }

        var tcs = new TaskCompletionSource<TResult>();

        _context.Post(obj => {
            try
            {
                var result = workItem();
                tcs.SetResult(result);
            }catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem, "workItem");
        if (CheckAccess())
        {
            return workItem();
        }
        var tcs = new TaskCompletionSource<TResult>();

        _context.Post(async obj => {
            try
            {
                var result = await workItem().ConfigureAwait(false);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }
    public void Tick()
    {
        _context.Tick();
    }
}
public class SingleThreadSyncContext : SynchronizationContext
{
    public static SingleThreadSyncContext Initialize()
    {
        var ctx = new SingleThreadSyncContext();
        SetSynchronizationContext(ctx);
        return ctx;
    }

    readonly List<TaskInfo> _taskList = [];

    readonly List<TaskInfo> _tempList = [];

    private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;
    public void Tick()
    {
        lock (_taskList)
        {
            _tempList.AddRange(_taskList);
            _taskList.Clear();
        }
        foreach (var task in _tempList)
        {
            task.Invoke();
        }
        _tempList.Clear();
    }
    public override void Post(SendOrPostCallback d, object? state)
    {
        lock (_taskList)
        {
            _taskList.Add(new TaskInfo
            {
                CallBack = d,
                State = state
            });
        }
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        if (Thread.CurrentThread.ManagedThreadId == _threadId)
        {
            d(state);
            return;
        }
        using var waitHandle = new ManualResetEvent(false);
        lock (_taskList)
        {
            _taskList.Add(new TaskInfo
            {
                CallBack = d,
                State = state,
                WaitHandle = waitHandle
            });
        }
        waitHandle.WaitOne();
    }

}

struct TaskInfo
{
    public SendOrPostCallback CallBack;
    public object? State;
    public ManualResetEvent? WaitHandle;

    public void Invoke()
    {
        try
        {
            CallBack?.Invoke(State);
        }
        finally
        {
            WaitHandle?.Set();
        }
    }

}