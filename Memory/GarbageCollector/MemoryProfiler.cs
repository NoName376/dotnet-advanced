using System.Runtime;

namespace Memory.GarbageCollector;

public static class MemoryProfilerRuner
{
    public static void Run()
    {
        GenerationStressTest.Run();
        FragmentationDemo.Run();
    }
}

public static class MemoryProfiler
{
    public static void PrintStats(string label)
    {
        var info = GC.GetGCMemoryInfo();
        Console.WriteLine($"\n[{label}]");
        Console.WriteLine($"Total memory : {GC.GetTotalMemory(false) / 1024,8} KB");
        Console.WriteLine($"Gen0 collections: {GC.CollectionCount(0)}");
        Console.WriteLine($"Gen1 collections: {GC.CollectionCount(1)}");
        Console.WriteLine($"Gen2 collections: {GC.CollectionCount(2)}");
        Console.WriteLine($"Heap size       : {info.HeapSizeBytes / 1024,8} KB");
        Console.WriteLine($"Fragmented      : {info.FragmentedBytes / 1024,8} KB");
    }
}

public static class GenerationStressTest
{
    private static readonly List<byte[]> _longLived = new();

    public static void Run()
    {
        MemoryProfiler.PrintStats("Start");

        Console.WriteLine("\n(Gen0): ");
        for (int i = 0; i < 10_000; i++)
            _ = new byte[128];
        GC.Collect(0);
        MemoryProfiler.PrintStats("After Gen0 stress");

        Console.WriteLine("\n(Gen0 → Gen1):");
        var mediumRefs = new List<WeakReference>();
        for (int i = 0; i < 1000; i++)
        {
            var obj = new byte[512];
            mediumRefs.Add(new WeakReference(obj));
            if (i % 3 != 0) continue;
            GC.Collect(0);
        }

        var alive = mediumRefs.Count(r => r.IsAlive);
        Console.WriteLine($"Medium-lived still alive after Gen0: {alive}/{mediumRefs.Count}");
        GC.Collect(1);
        
        alive = mediumRefs.Count(r => r.IsAlive);
        Console.WriteLine($"Medium-lived still alive after Gen1: {alive}/{mediumRefs.Count}");
        MemoryProfiler.PrintStats("After Gen1 stress");

        Console.WriteLine("\nLong-lived (Gen2 + LOH): ");
        for (int i = 0; i < 50; i++)
            _longLived.Add(new byte[100_000]);
        GC.Collect(2);
        MemoryProfiler.PrintStats("After Gen2/LOH stress");
    }
}

public static class FragmentationDemo
{
    public static void Run()
    {
        Console.WriteLine("\nFragmentation: allocate + release alternating");
        var anchors = new List<byte[]>();

        for (int i = 0; i < 500; i++)
        {
            anchors.Add(new byte[85_000]);
            _ = new byte[85_000];
        }

        MemoryProfiler.PrintStats("After fragmentation");

        anchors.Clear();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: false);
        MemoryProfiler.PrintStats("After GC without compaction");

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        MemoryProfiler.PrintStats("After GC WITH LOH compaction");
    }
}

