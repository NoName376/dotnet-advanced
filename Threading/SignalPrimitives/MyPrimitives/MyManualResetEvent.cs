namespace Threading.SignalPrimitives.MyPrimitives;

public class MyManualResetEvent
{
    public MyManualResetEvent(bool initialState)
    {
        _signaled = initialState;
    }
    
    private readonly object _lock = new();
    private bool _signaled;
    
    public void Set()
    {
        lock (_lock)
        {
            _signaled = true;
            Monitor.PulseAll(_lock);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _signaled = false;
        }
    }

    public void Wait(CancellationToken ct = default)
    {
        lock (_lock)
        {
            while (!_signaled)
            {
                ct.ThrowIfCancellationRequested();
                Monitor.Wait(_lock, millisecondsTimeout: 50);
            }
        }
    }

    public bool Wait(TimeSpan timeout, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (!_signaled)
            {
                ct.ThrowIfCancellationRequested();
                
                var remaining = deadline - DateTime.UtcNow;
                
                if (remaining <= TimeSpan.Zero)
                    return false;
                
                Monitor.Wait(_lock, remaining);
            }
            return true;
        }
    }

    public bool IsSet
    {
        get
        {
            lock (_lock)
            {
                return _signaled;
            }
        }
    }
}