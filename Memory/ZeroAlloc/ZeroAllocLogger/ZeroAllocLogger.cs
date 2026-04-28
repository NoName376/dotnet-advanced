using System.Buffers;
using System.Diagnostics;

namespace Memory.ZeroAlloc.ZeroAllocLogger;

public sealed class ZeroAllocLogger : IDisposable
{
    public ZeroAllocLogger(TextWriter writer)
    {
        _writer = writer;
    }
    
    public void Log(Severity severity, ReadOnlySpan<char> message)
    {
        using var owner = new PooledBuffer(BufferSize);
        var span = owner.Memory.Span;
        int pos = 0;

        WriteTimestamp(span, ref pos);
        WriteSpan(span, ref pos, " [");
        WriteSpan(span, ref pos, SeverityLabel(severity));
        WriteSpan(span, ref pos, "] ");
        WriteSpan(span, ref pos, message);

        _writer.Write(span[..pos]);
        _writer.Write(Environment.NewLine);
    }

    private static void WriteTimestamp(Span<char> buf, ref int pos)
    {
        var now = DateTime.Now;
        now.TryFormat(buf[pos..], out int written, "HH:mm:ss.fff");
        pos += written;
    }

    private static void WriteSpan(Span<char> buf, ref int pos, ReadOnlySpan<char> value)
    {
        value.CopyTo(buf[pos..]);
        pos += value.Length;
    }

    private static ReadOnlySpan<char> SeverityLabel(Severity s) => s switch
    {
        Severity.Debug => "DBG",
        Severity.Info  => "INF",
        Severity.Warn  => "WRN",
        Severity.Error => "ERR",
        _              => "???"
    };

    public static void BenchmarkVsNaive(int iterations)
    {
        var message = "User logged in from 192.168.1.1".AsSpan();

        Console.WriteLine("\nZeroAllocLogger");
        using var zeroLogger = new ZeroAllocLogger(TextWriter.Null);
        
        var gcBefore = (GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        long allocBefore = GC.GetAllocatedBytesForCurrentThread();
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            zeroLogger.Log(Severity.Info, message);
        sw.Stop();
        
        long allocAfter = GC.GetAllocatedBytesForCurrentThread();
        var gcAfter = (GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Allocated: {(allocAfter - allocBefore) / 1024} KB");
        Console.WriteLine($"GC Gen0: {gcAfter.Item1 - gcBefore.Item1}; Gen1: {gcAfter.Item2 - gcBefore.Item2}; Gen2: {gcAfter.Item3 - gcBefore.Item3}");

        
        Console.WriteLine("\nNaive logger");
        gcBefore = (GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        allocBefore = GC.GetAllocatedBytesForCurrentThread();
        
        sw.Restart();
        for (int i = 0; i < iterations; i++)
            TextWriter.Null.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - {new string(message)}");
        sw.Stop();

        allocAfter = GC.GetAllocatedBytesForCurrentThread();
        gcAfter = (GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Allocated: {(allocAfter - allocBefore) / 1024} KB");
        Console.WriteLine($"GC Gen0: {gcAfter.Item1 - gcBefore.Item1}; Gen1: {gcAfter.Item2 - gcBefore.Item2}; Gen2: {gcAfter.Item3 - gcBefore.Item3}");}

    private readonly TextWriter _writer;
    private const int BufferSize = 512;
    
    public void Dispose() 
        => _writer.Dispose();
}