using Memory.GarbageCollector;

namespace Memory;

class Program
{
    static void Main(string[] args)
    {
        GcHandlePinning.Run();
    }
}