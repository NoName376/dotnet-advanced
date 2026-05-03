namespace Threading.SignalPrimitives.ConfigService;

public class ConfigService : IDisposable
{
    private readonly ManualResetEventSlim _gate = new(initialState: true);
    private volatile AppConfig            _config = new();
    private int                           _pausedWorkers;

    public AppConfig Current => _config;

    public void Wait(CancellationToken ct) => _gate.Wait(ct);

    public void Reload(AppConfig next)
    {
        Console.WriteLine("\n[Config] Closing gate");
        _gate.Reset();

        Thread.Sleep(500);

        int waited = 0;
        while (Volatile.Read(ref _pausedWorkers) < 1 && waited < 2000)
        {
            Thread.Sleep(50);
            waited += 50;
        }

        Console.WriteLine($"[Config] Updating config (paused workers: {_pausedWorkers})");
        _config = next;
        Thread.Sleep(300);

        Console.WriteLine("[Config] Opening gate\n");
        _gate.Set();
    }

    public void IncrementPaused() => Interlocked.Increment(ref _pausedWorkers);
    public void DecrementPaused() => Interlocked.Decrement(ref _pausedWorkers);
    
    public int  PausedWorkers => _pausedWorkers;

    public void Dispose() => _gate.Dispose();
}