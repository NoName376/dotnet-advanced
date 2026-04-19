namespace TPL.ThreadPool;

public static class CustomThreadPoolTask
{
    public static class ThreadPoolRun
    {
        public static void Run()
        {
            Console.WriteLine("Basics of Thread Pool");
            using (var pool = new CustomThreadPool(3))
            {
                for (int i = 1; i <= 6; i++)
                {
                    var taskId = i;
                    pool.Queue(() =>
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] Task {taskId} started");
                        Thread.Sleep(300);
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] Task {taskId} done");
                    });
                }
            }
            
            Console.WriteLine("\nException handling");
            using (var pool = new CustomThreadPool(2))
            {
                pool.Queue(() => throw new InvalidOperationException("Simulated failure"));
            }
            

            Console.WriteLine("\nQueue after Dispose");
            var disposed = new CustomThreadPool(2);
            
            disposed.Dispose();
            disposed.Queue(() => Console.WriteLine("This should never print"));
            
            Console.WriteLine("\nAll demos finished.");
        }
    }

    public class CustomThreadPool : IDisposable
    {
        public CustomThreadPool(int maxThreads)
        {
            _maxThreads = maxThreads;
            _threads = new Thread[_maxThreads];

            for (int i = 0; i < _maxThreads; i++)
            {
                _threads[i] = new Thread(ThreadProc)
                {
                    IsBackground = true,
                    Name = $"{nameof(CustomThreadPool)}: #{i + 1}"
                };
                _threads[i].Start();
            }
        }

        private readonly Thread[] _threads;
        private readonly int _maxThreads;

        private readonly Queue<Action> _queue = new();
        private readonly object _lock = new();

        private volatile bool _disposed;

        public void Queue(Action action)
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_disposed) return;

                _queue.Enqueue(action);
                Monitor.Pulse(_lock);
            }
        }

        private void ThreadProc()
        {
            while (true)
            {
                Action action;
                lock (_lock)
                {
                    while (_queue.Count == 0 && !_disposed)
                        Monitor.Wait(_lock);

                    if (_disposed && _queue.Count == 0)
                        return;

                    action = _queue.Dequeue();
                }

                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[{Thread.CurrentThread.Name}] Error: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_disposed) return;

                _disposed = true;
                Monitor.PulseAll(_lock);
            }

            foreach (var thread in _threads)
                thread.Join();
        }
    }
}