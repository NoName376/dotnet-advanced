using System.Runtime.InteropServices;

namespace Memory.GarbageCollector;

public static class GcHandlePinning
{
    public static void Run()
    {
        int[] managedArray = { 1, 2, 3, 4, 5 };
        
        GCHandle handle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);

        try
        {
            IntPtr address = handle.AddrOfPinnedObject();
            Console.WriteLine($"Array pinned at: 0x{address:X}");

            // Simulate unmanaged modification
            IntPtr elementPtr = IntPtr.Add(address, sizeof(int) * 2); 
            Marshal.WriteInt32(elementPtr, 999);

            Console.WriteLine($"Managed array index 2 is now: {managedArray[2]}"); 
        }
        finally
        {
            if (handle.IsAllocated)
            {
                handle.Free();
                Console.WriteLine("Array unpinned.");
            }
        }
    }
}