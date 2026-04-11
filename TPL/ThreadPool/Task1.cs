using BenchmarkDotNet.Attributes;

namespace TPL.ThreadPool;

public static class Task1
{
    public const int ThreadCount = 8;
    public static int Counter;
    
    public static void RaceConditional()
    {
        Counter = 0;
        var threads = new Thread[ThreadCount]; 
        
        // Race Conditional
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    Counter++;
                }
            });
            
            threads[i].Start();
        }
        
        foreach (var thr in threads)
            thr.Join();
            
        Console.WriteLine($"Result (RaceConditional): {Counter}");
    }

    public static void LockUsage()
    {
        Counter = 0;
        
        var obj = new object();
        var threads = new Thread[ThreadCount]; 
        
        // Race Conditional
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    lock (obj)
                    {
                        Counter++;
                    }
                }
            });
            
            threads[i].Start();
        }
        
        foreach (var thr in threads)
            thr.Join();
            
        Console.WriteLine($"Result (LockUsage): {Counter}");
    }

    public static void InterlockedUsage()
    {
        Counter = 0;
        var threads = new Thread[ThreadCount]; 
        
        // Race Conditional
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    Interlocked.Add(ref Counter, 1);
                }
            });
            
            threads[i].Start();
        }
        
        foreach (var thr in threads)
            thr.Join();
            
        Console.WriteLine($"Result (InterlockUsage): {Counter}");
    }

    public static void MonitorUsage()
    {
        Counter = 0;
        
        var obj = new object();
        var threads = new Thread[ThreadCount]; 
        
        // Race Conditional
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    Monitor.Enter(obj);
                    Counter++;
                    Monitor.Exit(obj);
                }
            });
            
            threads[i].Start();
        }
        
        foreach (var thr in threads)
            thr.Join();
            
        Console.WriteLine($"Result (MonitorUsage): {Counter}");
    }
    
    public static void MutexUsage()
    {
        Counter = 0;

        using var mutex = new Mutex();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    mutex.WaitOne();
                    Counter++;
                    mutex.ReleaseMutex();
                }
            });

            threads[i].Start();
        }

        foreach (var thr in threads)
            thr.Join();

        Console.WriteLine($"Result (MutexUsage): {Counter}");
    }
    
    public static void SemaphoreUsage()
    {
        Counter = 0;

        using var semaphore = new Semaphore(1, 1);
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    semaphore.WaitOne();
                    Counter++;
                    semaphore.Release();
                }
            });

            threads[i].Start();
        }

        foreach (var thr in threads)
            thr.Join();

        Console.WriteLine($"Result (SemaphoreUsage): {Counter}");
    }
    
    public static void SemaphoreSlimUsage()
    {
        Counter = 0;

        var semaphore = new SemaphoreSlim(1, 1);
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    semaphore.Wait();
                    Counter++;
                    semaphore.Release();
                }
            });

            threads[i].Start();
        }

        foreach (var thr in threads)
            thr.Join();

        Console.WriteLine($"Result (SemaphoreSlimUsage): {Counter}");
    }
    
    public static void SpinLockUsage()
    {
        Counter = 0;

        var spinLock = new SpinLock();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    bool taken = false;
                    try
                    {
                        spinLock.Enter(ref taken);
                        Counter++;
                    }
                    finally
                    {
                        if (taken)
                            spinLock.Exit();
                    }
                }
            });

            threads[i].Start();
        }

        foreach (var thr in threads)
            thr.Join();

        Console.WriteLine($"Result (SpinLockUsage): {Counter}");
    }
    
    public static void ReaderWriterLockUsage()
    {
        Counter = 0;

        var rwLock = new ReaderWriterLockSlim();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    rwLock.EnterWriteLock();
                    try
                    {
                        Counter++;
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
            });

            threads[i].Start();
        }

        foreach (var t in threads)
            t.Join();

        Console.WriteLine($"Result (RWLock): {Counter}");
    }
}