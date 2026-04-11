using TPL.ThreadPool;

namespace TPL;

class Program
{
    static void Main(string[] args)
    {
        Task1.RaceConditional();
        Task1.LockUsage();
        Task1.InterlockedUsage();
        Task1.MonitorUsage();
        Task1.MutexUsage();
        Task1.SemaphoreUsage();
        Task1.SemaphoreSlimUsage();
        Task1.SpinLockUsage();
        Task1.ReaderWriterLockUsage();
    }
}