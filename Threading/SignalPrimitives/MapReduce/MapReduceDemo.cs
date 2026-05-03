namespace Threading.SignalPrimitives.MapReduce;

public static class MapReduceDemo
{
    public static void Run()
    {
        using var runner = new MapReduce(mappers: 4, dataSize: 100);
        runner.RunPhases(phases: 3);
    }
}