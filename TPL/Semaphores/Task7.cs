namespace TPL.Semaphores;

public static class Task7Runner
{
    public static void Run()
    {
        using var pool = new Task7.ConnectionPool(3);

        var workers = Enumerable.Range(1, 10).Select(id => new Thread(() =>
        {
            Console.WriteLine($"[Worker {id}] Waiting for connection...");
            var conn = pool.Acquire();
            Console.WriteLine($"[Worker {id}] Got connection {conn.Id}");
            conn.Execute($"SELECT * FROM orders WHERE worker = {id}");
            pool.Release(conn);
            Console.WriteLine($"[Worker {id}] Released connection {conn.Id}");
        }) { Name = $"Worker-{id}" }).ToList();

        workers.ForEach(w => w.Start());
        workers.ForEach(w => w.Join());

        Console.WriteLine("\nAll workers done. Pool disposed cleanly.");
    }
}

public class Task7
{
    public class ManualSemaphore
    {
        public ManualSemaphore(int initialCount)
        {
            _count = initialCount;
        }
        
        private readonly object _lock = new();
        private int _count;

        public void Wait()
        {
            lock (_lock)
            {
                while (_count == 0)
                    Monitor.Wait(_lock);
                _count--;
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                _count++;
                Monitor.Pulse(_lock);
            }
        }

        public int CurrentCount
        {
            get
            {
                lock (_lock)
                {
                    return _count;
                }
            }
        }
    }

    public class DbConnection
    {
        private static int _idCounter;
        public int Id { get; } = Interlocked.Increment(ref _idCounter);

        public void Execute(string query)
        {
            Console.WriteLine($"  [Connection {Id}] Executing: {query}");
            Thread.Sleep(300);
            Console.WriteLine($"  [Connection {Id}] Done: {query}");
        }
    }

    public class ConnectionPool : IDisposable
    {
        public ConnectionPool(int size)
        {
            _semaphore = new ManualSemaphore(size);
            for (int i = 0; i < size; i++) _connections.Push(new DbConnection());
        }
        
        private readonly ManualSemaphore _semaphore;
        private readonly Stack<DbConnection> _connections = new();
        private readonly object _lock = new();
        private bool _disposed;
        
        public DbConnection Acquire()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _semaphore.Wait();
            lock (_lock)
            {
                return _connections.Pop();
            }
        }

        public void Release(DbConnection connection)
        {
            lock (_lock)
            {
                _connections.Push(connection);
            }

            _semaphore.Release();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            while (_semaphore.CurrentCount < _connections.Count) Thread.Sleep(10);
        }
    }
}