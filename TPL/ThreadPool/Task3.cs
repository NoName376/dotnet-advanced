using System.Collections;
using System.Collections.Concurrent;

namespace TPL.ThreadPool;

public static class Task3Runner
{
    public static void Run()
    {
        using var queue = new Task3.TaskQueue<int>();

        var producer = new Task3.Producer(queue);
        var consumer1 = new Task3.Consumer(queue, 1);
        var consumer2 = new Task3.Consumer(queue, 2);

        producer.Start();
        consumer1.Start();
        consumer2.Start();

        producer.Join();
        consumer1.Join();
        consumer2.Join();

        Console.WriteLine("All done!");
    }
}

public class Task3
{
    public class TaskQueue<T> : IEnumerable<T>, IDisposable
    {
        public TaskQueue()
        {
            _collection = new BlockingCollection<T>(new ConcurrentQueue<T>());
        }
        
        private readonly BlockingCollection<T> _collection;

        public void Enqueue(T item)
        {
            _collection.Add(item);
        }

        public void CompleteAdding()
        {
            _collection.CompleteAdding();
        }

        public void Dispose()
        {
            _collection.Dispose();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetConsumingEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public class Producer
    {
        public Producer(TaskQueue<int> queue)
        {
            _queue = queue;
            _thread = new Thread(Run);
        }
        
        private readonly TaskQueue<int> _queue;
        private readonly Thread _thread;

        public void Start() => _thread.Start();

        private void Run()
        {
            for (int i = 1; i <= 20; i++)
            {
                Console.WriteLine($"[Producer] Added: {i}");
                _queue.Enqueue(i);
                
                Thread.Sleep(100);
            }
            
            _queue.CompleteAdding();
        }

        public void Join() => _thread.Join();
    }
    public class Consumer
    {
        private readonly TaskQueue<int> _queue;
        private readonly int _id;
        private readonly Thread _thread;

        public Consumer(TaskQueue<int> queue, int id)
        {
            _queue = queue;
            _id = id;
            _thread = new Thread(Run);
        }

        public void Start() => _thread.Start();

        private void Run()
        {
            foreach (var item in _queue)
            {
                Console.WriteLine($"[Consumer {_id}] Processed: {item}");
            }

            Console.WriteLine($"[Consumer {_id}] Finished");
        }

        public void Join() => _thread.Join();
    }
}