namespace TPL.Semaphores;

public static class Task6Run
{
    public static void Run()
    {
        var book = new Task6.OrderBook();
        var rng = new Random();

        book.AddOrder(100.5m, 10, isBid: true);
        book.AddOrder(100.0m, 25, isBid: true);
        book.AddOrder(99.5m, 15, isBid: true);
        book.AddOrder(101.0m, 8, isBid: false);
        book.AddOrder(101.5m, 20, isBid: false);
        book.AddOrder(102.0m, 12, isBid: false);

        var readers = Enumerable.Range(1, 4).Select(id => new Thread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                var (bids, asks) = book.GetTop5();
                Console.WriteLine(
                    $"[Reader {id}] Top bid: {bids[0].Price} x{bids[0].Qty} " +
                    $"| Top ask: {asks[0].Price} x{asks[0].Qty}");
                Thread.Sleep(rng.Next(50, 150));
            }
        }) { Name = $"Reader-{id}" }).ToList();

        var writer = new Thread(() =>
        {
            decimal[] prices = { 100.2m, 100.8m, 101.2m, 99.8m, 101.8m };
            foreach (var price in prices)
            {
                bool isBid = price < 101m;
                book.AddOrder(price, rng.Next(5, 30), isBid);
                Console.WriteLine($"[Writer] Added {(isBid ? "bid" : "ask")} @ {price}");
                Thread.Sleep(rng.Next(80, 200));
            }
        }) { Name = "Writer" };

        readers.ForEach(r => r.Start());
        writer.Start();
        readers.ForEach(r => r.Join());
        writer.Join();

        Console.WriteLine("\nOrder Book: ");
        var (finalBids, finalAsks) = book.GetTop5();
        Console.WriteLine("Bids:");
        finalBids.ForEach(b => Console.WriteLine($"  {b.Price,8} x{b.Qty}"));
        Console.WriteLine("Asks:");
        finalAsks.ForEach(a => Console.WriteLine($"  {a.Price,8} x{a.Qty}"));
    }
}

public class Task6
{
    public class OrderBook
    {
        private readonly SortedDictionary<decimal, int> _bids =
            new(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));

        private readonly SortedDictionary<decimal, int> _asks = new();

        private readonly object _sync = new();

        private int _readersCount;
        private int _waitingWriters;
        private bool _isWriting;
        
        public void AddOrder(decimal price, int quantity, bool isBid)
        {
            Monitor.Enter(_sync);
            try
            {
                _waitingWriters++;
                try
                {
                    while (_isWriting || _readersCount > 0)
                        Monitor.Wait(_sync);

                    _isWriting = true;
                }
                finally
                {
                    _waitingWriters--;
                }

                var book = isBid ? _bids : _asks;

                if (book.TryGetValue(price, out var existing))
                    book[price] = existing + quantity;
                else
                    book[price] = quantity;

                _isWriting = false;
                Monitor.PulseAll(_sync);
            }
            catch
            {
                _isWriting = false;
                Monitor.PulseAll(_sync);
                throw;
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }

        public void CancelOrder(decimal price, int quantity, bool isBid)
        {
            Monitor.Enter(_sync);
            try
            {
                _waitingWriters++;
                try
                {
                    while (_isWriting || _readersCount > 0)
                        Monitor.Wait(_sync);
                    _isWriting = true;
                }
                finally
                {
                    _waitingWriters--;
                }

                var book = isBid ? _bids : _asks;
                
                if (book.TryGetValue(price, out var existing))
                {
                    var remaining = existing - quantity;
                    
                    if (remaining <= 0) 
                        book.Remove(price);
                    else
                        book[price] = remaining;
                }

                _isWriting = false;
                Monitor.PulseAll(_sync);
            }
            catch
            {
                _isWriting = false;
                Monitor.PulseAll(_sync);
                throw;
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }
        
        public (List<(decimal Price, int Qty)> Bids, List<(decimal Price, int Qty)> Asks) GetTop5()
        {
            Monitor.Enter(_sync);
            try
            {
                while (_isWriting || _waitingWriters > 0)
                    Monitor.Wait(_sync);

                _readersCount++;
            }
            finally
            {
                Monitor.Exit(_sync);
            }
            
            try
            {
                return (
                    _bids.Take(5).Select(kv => (kv.Key, kv.Value)).ToList(),
                    _asks.Take(5).Select(kv => (kv.Key, kv.Value)).ToList()
                );
            }
            finally
            {
                Monitor.Enter(_sync);
                try
                {
                    _readersCount--;

                    if (_readersCount == 0)
                        Monitor.PulseAll(_sync);
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
        }
    }
}