namespace Memory.ZeroAlloc.ZeroAllocLogger;

public static class ZeroAllocLoggerDemo
{
    public static void Run()
    {
        using var logger = new ZeroAllocLogger(Console.Out);
        logger.Log(Severity.Info,  "Application started");
        logger.Log(Severity.Debug, "Connected to database");
        logger.Log(Severity.Warn,  "Warning!!!");
        logger.Log(Severity.Error, "Bug!");

        Console.WriteLine("\nBenchmark");
        ZeroAllocLogger.BenchmarkVsNaive(1_000_000);
    }
}