namespace TPL.SignalPrimitives.Primitives;

public class MyCountdownEvent
{
    public MyCountdownEvent(int initialCount)
    {
        if (initialCount < 0) 
            throw new ArgumentOutOfRangeException(nameof(initialCount));
        
        _count        = initialCount;
        _initialCount = initialCount;
    }
    
    private readonly object _lock = new();
    private int _count;
    private readonly int _initialCount;

    public void Signal()
    {
        lock (_lock)
        {
            if (_count <= 0) 
                throw new InvalidOperationException("CountdownEvent already zero");
            
            _count--;
            
            if (_count == 0)
                Monitor.PulseAll(_lock);
        }
    }

    public void Reset(int count)
    {
        lock (_lock)
        {
            _count = count;
        }
    }

    public void Reset() 
        => Reset(_initialCount);

    public void Wait(CancellationToken ct = default)
    {
        lock (_lock)
        {
            while (_count > 0)
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
            while (_count > 0)
            {
                ct.ThrowIfCancellationRequested();
                
                var remaining = deadline - DateTime.UtcNow;
                
                if (remaining <= TimeSpan.Zero) 
                    return _count == 0;
                
                Monitor.Wait(_lock, remaining);
            }
            return true;
        }
    }

    public int CurrentCount
    {
        get
        {
            lock (_lock)
            {
                return _count;
            }
        }
    }

    public bool IsSet
    {
        get
        {
            lock (_lock)
            {
                return _count == 0;
            }
        }
    }
}