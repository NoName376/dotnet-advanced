namespace Threading.SynchronizationPrimitives;

public static class Task8Runner
{
    public static void Run()
    {
        using var cache = new SmartCacheService.SmartCache<string, string>(
            loader: key =>
            {
                Console.WriteLine($"  [Loader] Fetching '{key}' from DB (mock)");
                Thread.Sleep(500);
                return $"value_of_{key}_{DateTime.UtcNow:ss.fff}";
            },
            ttl: TimeSpan.FromSeconds(3),
            invalidationInterval: TimeSpan.FromSeconds(2)
        );

        Console.WriteLine("Parallel cache miss");
        var threads = Enumerable.Range(1, 5).Select(i => new Thread(() =>
        {
            var result = cache.Get("user:42");
            Console.WriteLine($"  [Thread {i}] Got: {result}");
        }) { Name = $"T-{i}" }).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        Console.WriteLine("\nSecond read");
        Enumerable.Range(1, 3).ToList().ForEach(_ =>
            Console.WriteLine($"  Got: {cache.Get("user:42")}"));

        Console.WriteLine("\nWaiting for TTL expiry");
        Thread.Sleep(4000);

        Console.WriteLine("\nAfter expiry");
        Console.WriteLine($"  Got: {cache.Get("user:42")}");
    }
}

public class SmartCacheService
{
    public class CacheEntry<TValue>
    {
        public CacheEntry(TValue value, TimeSpan ttl)
        {
            Value = value;
            ExpiresAt = DateTime.UtcNow + ttl;
        }
        
        public TValue Value { get; }
        public DateTime ExpiresAt { get; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    public class SmartCache<TKey, TValue> : IDisposable where TKey : notnull
    {
        public SmartCache(Func<TKey, TValue> loader, TimeSpan ttl, TimeSpan invalidationInterval)
        {
            _loader = loader;
            _ttl = ttl;
            _invalidationInterval = invalidationInterval;

            _invalidator = new Thread(InvalidationLoop) { IsBackground = true, Name = "Cache-Invalidator" };
            _invalidator.Start();
        }
        
        private readonly Func<TKey, TValue> _loader;
        private readonly TimeSpan _ttl;
        private readonly TimeSpan _invalidationInterval;

        private readonly Dictionary<TKey, CacheEntry<TValue>> _store = new();
        private readonly Dictionary<TKey, SemaphoreSlim> _loadLocks = new();
        private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);

        private readonly CancellationTokenSource _cts = new();
        private readonly Thread _invalidator;

        public TValue Get(TKey key)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_store.TryGetValue(key, out var entry) && !entry.IsExpired)
                {
                    Console.WriteLine($"  [Cache] HIT for key={key} on [{Thread.CurrentThread.Name}]");
                    return entry.Value;
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            return LoadAndCache(key);
        }

        private TValue LoadAndCache(TKey key)
        {
            SemaphoreSlim keyLock;

            _rwLock.EnterWriteLock();
            try
            {
                if (_store.TryGetValue(key, out var fresh) && !fresh.IsExpired) return fresh.Value;

                if (!_loadLocks.TryGetValue(key, out keyLock))
                {
                    keyLock = new SemaphoreSlim(1, 1);
                    _loadLocks[key] = keyLock;
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            keyLock.Wait();
            try
            {
                _rwLock.EnterReadLock();
                try
                {
                    if (_store.TryGetValue(key, out var cached) && !cached.IsExpired)
                    {
                        Console.WriteLine($"  [Cache] SECOND-CHECK HIT for key={key} on [{Thread.CurrentThread.Name}]");
                        return cached.Value;
                    }
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }

                Console.WriteLine($"  [Cache] MISS: loading key={key} on [{Thread.CurrentThread.Name}]");
                var value = _loader(key);

                _rwLock.EnterWriteLock();
                try
                {
                    _store[key] = new CacheEntry<TValue>(value, _ttl);
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                return value;
            }
            finally
            {
                keyLock.Release();
            }
        }

        private void InvalidationLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                Thread.Sleep(_invalidationInterval);

                _rwLock.EnterWriteLock();
                try
                {
                    var expired = _store.Where(kv => kv.Value.IsExpired).Select(kv => kv.Key).ToList();
                    foreach (var key in expired)
                    {
                        _store.Remove(key);
                        _loadLocks.Remove(key);
                        Console.WriteLine($"  [Invalidator] Evicted key={key}");
                    }
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _invalidator.Join();
            _rwLock.Dispose();
            _cts.Dispose();
            foreach (var s in _loadLocks.Values) s.Dispose();
        }
    }
}