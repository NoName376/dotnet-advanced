using System.Runtime.InteropServices;

namespace Memory.GarbageCollector;

public static class NativeBufferRunner
{
    public static void Run()
    {
        Console.WriteLine("Correct: using");
        using (var pool = new MemoryLeak(1024, 3))
        {
            using var buf = pool.Rent();
            Console.WriteLine($"Working with buffer at 0x{buf.Ptr:X}");
        }

        Console.WriteLine("\nLeak: forgot Dispose");
        CreateLeak();

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Console.WriteLine($"\nTotal leaks caught by finalizer: {NativeBuffer.LeakCounter}");
    }
    
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void CreateLeak()
    {
        var pool = new MemoryLeak(1024, 2);
        var buf = pool.Rent();
        Console.WriteLine($"  Got buffer at 0x{buf.Ptr:X}");
    }
}

public unsafe class NativeBuffer : IDisposable
{
    public NativeBuffer(int size)
    {
        _size = size;
        _ptr = Marshal.AllocHGlobal(size);
        Console.WriteLine($"[NativeBuffer] Allocated {size} bytes at 0x{_ptr:X}");
    }
    ~NativeBuffer()
    {
        if (!_disposed)
        {
            Interlocked.Increment(ref _leakCounter);
            Console.WriteLine($"[Finalizer] MEMORY LEAK DETECTED! Cleaning up 0x{_ptr:X}");
            Free();
        }
    }
    
    private static int _leakCounter;
    public static int LeakCounter => _leakCounter;

    private IntPtr _ptr;
    private readonly int _size;
    private bool _disposed;

    public IntPtr Ptr => _disposed ? throw new ObjectDisposedException(nameof(NativeBuffer)) : _ptr;
    public int Size => _size;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Free();
        GC.SuppressFinalize(this);
    }

    private void Free()
    {
        if (_ptr == IntPtr.Zero) return;
        Marshal.FreeHGlobal(_ptr);
        _ptr = IntPtr.Zero;
        Console.WriteLine($"[NativeBuffer] Freed {_size} bytes. No Memory Leak");
    }
}
public class MemoryLeak : IDisposable
{
    public MemoryLeak(int bufferSize, int count)
    {
        _bufferSize = bufferSize;
        for (int i = 0; i < count; i++)
            _pool.Push(new NativeBuffer(bufferSize));
    }
    
    private readonly Stack<NativeBuffer> _pool = new();
    private readonly object _lock = new();
    private readonly int _bufferSize;
    
    private bool _disposed;
    
    public NativeBuffer Rent()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            
            return _pool.Count > 0 ? 
                _pool.Pop() : 
                new NativeBuffer(_bufferSize);
        }
    }

    public void Return(NativeBuffer buffer)
    {
        lock (_lock)
        {
            if (_disposed) { buffer.Dispose(); return; }
            _pool.Push(buffer);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        lock (_lock)
        {
            if (_disposed) 
                return;
            
            _disposed = true;
            while (_pool.Count > 0)
                _pool.Pop().Dispose();
        }
    }
}

