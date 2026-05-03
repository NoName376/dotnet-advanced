namespace Threading.SignalPrimitives.ConfigService;

public class Worker
{
    public Worker(int id, ConfigService config)
    {
        _id     = id;
        _config = config;
        _thread = new Thread(Run) { Name = $"Worker_{id}" };
    }
    
    private readonly int           _id;
    private readonly ConfigService _config;
    private readonly Thread        _thread;

    public void Start() => _thread.Start();
    public void Join() => _thread.Join();

    private void Run()
    {
        using var cts = new CancellationTokenSource();

        try
        {
            for (int i = 0; i < 8; i++)
            {
                _config.IncrementPaused();
                _config.Wait(cts.Token);
                _config.DecrementPaused();

                var cfg = _config.Current;
                Console.WriteLine($"[Worker {_id}] iter={i} env={cfg.Environment} retries={cfg.MaxRetries}");
                Thread.Sleep(200);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Worker {_id}] Cancelled while waiting for config");
        }
    }
}