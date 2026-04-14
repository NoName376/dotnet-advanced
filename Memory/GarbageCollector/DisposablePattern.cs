using System.Runtime.InteropServices;

namespace Memory.GarbageCollector;

public static class DisposablePattern
{
    public static void Run()
    {
        usingContext:
        {
            var ms = new MemoryStream(); // Dispose control for this object has been transferred to the Resource Wrapper class
            using var resource = new AdvancedResourceWrapper(ms);
            
            resource.DoWork(); 
        }

        using (var res = new MySafeResourceHandle())
        {
            Console.WriteLine("Usage of some resources...");
        }
        
        Console.WriteLine("Object Disposed!");
    }
}

public sealed class MySafeResourceHandle : SafeHandle
{
    public MySafeResourceHandle() : base(IntPtr.Zero, ownsHandle: true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        Console.WriteLine($"[SafeHandle] Releasing handle: 0x{handle.ToInt64():X}");

        return true;
    }
}

public class AdvancedResourceWrapper : IDisposable
{
    public AdvancedResourceWrapper(IDisposable managedResource)
    {
        _managedResource = managedResource;
        _unmanagedResource = Marshal.AllocHGlobal(1024);
        Console.WriteLine("Resource initialized.");
    }
    
    private IntPtr _unmanagedResource;
    private IDisposable? _managedResource;
    private bool _isDisposed;

   
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            Console.WriteLine("Releasing managed resources");
            _managedResource?.Dispose();
            _managedResource = null;
        }

        if (_unmanagedResource != IntPtr.Zero)
        {
            Console.WriteLine("Releasing unmanaged memory");
            Marshal.FreeHGlobal(_unmanagedResource);
            _unmanagedResource = IntPtr.Zero;
        }

        _isDisposed = true;
    }
    
    ~AdvancedResourceWrapper()
    {
        Console.WriteLine("Finalizer called.");
        Dispose(false);
    }

    public void DoWork()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(AdvancedResourceWrapper));
        Console.WriteLine("Doing work with resources...");
    }
}
