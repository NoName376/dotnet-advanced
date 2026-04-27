namespace Memory.ZeroAlloc.BinaryProtocolParser;

public static class PacketParserDemo
{
    public static void Run()
    {
        var buffer = PacketProcessor.BuildTestBuffer(3);

        PacketProcessor.Process(buffer);

        Console.WriteLine("\nBenchmark");
        var bigBuffer = PacketProcessor.BuildTestBuffer(100);
        PacketProcessor.BenchmarkVsNaive(bigBuffer, 10_000);
    }
}