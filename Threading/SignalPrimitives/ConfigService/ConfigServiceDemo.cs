namespace Threading.SignalPrimitives.ConfigService;

public static class ConfigServiceDemo
{
    public static void Run()
    {
        using var config = new ConfigService();

        var workers = Enumerable.Range(1, 4)
            .Select(i => new Worker(i, config))
            .ToList();

        workers.ForEach(w => w.Start());
        Thread.Sleep(400);

        config.Reload(new AppConfig
        {
            DatabaseUrl = "db://replica:5432",
            MaxRetries  = 5,
            Environment = "staging"
        });

        Thread.Sleep(600);

        config.Reload(new AppConfig
        {
            DatabaseUrl = "db://primary:5432",
            MaxRetries  = 7,
            Environment = "production-v2"
        });

        workers.ForEach(w => w.Join());
    }
}