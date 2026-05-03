namespace Threading.SignalPrimitives.MyPrimitives;

public class MyAutoResetEvent
{
    public MyAutoResetEvent(bool initialState)
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
            Monitor.Pulse(_lock);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _signaled = false;
        }
    }

    public bool WaitOne(TimeSpan timeout)
    {
        lock (_lock)
        {
            var deadline = DateTime.UtcNow + timeout;
            
            while (!_signaled)
            {
                var remaining = deadline - DateTime.UtcNow;
                
                if (remaining <= TimeSpan.Zero)
                    return false;
                
                Monitor.Wait(_lock, remaining);
            }
            
            _signaled = false;
            return true;
        }
    }

    public void WaitOne()
    {
        WaitOne(Timeout.InfiniteTimeSpan);
    }

    public bool WaitOne(TimeSpan timeout, CancellationToken ct)
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
            
            _signaled = false;
            return true;
        }
    }
}