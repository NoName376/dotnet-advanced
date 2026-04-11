using System.Collections;

namespace TPL.ThreadPool;

public static class Task4Runner
{
    public static void Run()
    {
        var queue = new Task4.TaskQueue<int>();
        
        var producer = new Task4.Producer(queue);
        var consumer1 = new Task4.Consumer(queue, 1);
        var consumer2 = new Task4.Consumer(queue, 2);
        
        producer.Start();
        consumer1.Start();
        consumer2.Start();
        
        producer.Join();
        consumer1.Join();
        consumer2.Join();
        
        Console.WriteLine("All done!");
    }
}

public class Task4
{
    public class TaskQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue = new();
        private readonly object _syncRoot = new();
        private bool _completed;

        public void Enqueue(T item)
        {
            lock (_syncRoot)
            {
                _queue.Enqueue(item);
                Monitor.PulseAll(_syncRoot);
            }
        }

        public void CompleteAdding()
        {
            lock (_syncRoot)
            {
                _completed = true;
                Monitor.PulseAll(_syncRoot);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (true)
            {
                T item;
                lock (_syncRoot)
                {
                    while (_queue.Count == 0 && !_completed)
                        Monitor.Wait(_syncRoot);

                    if (_queue.Count == 0)
                        yield break;

                    item = _queue.Dequeue();
                }
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Producer
    {
        private readonly TaskQueue<int> _queue;
        private readonly Thread _thread;

        public Producer(TaskQueue<int> queue)
        {
            _queue = queue;
            _thread = new Thread(Run);
        }

        public void Start() => _thread.Start();
        public void Join() => _thread.Join();

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
        public void Join() => _thread.Join();

        private void Run()
        {
            foreach (var item in _queue)
                Console.WriteLine($"[Consumer {_id}] Processed: {item}");

            Console.WriteLine($"[Consumer {_id}] Finished");
        }
    }
}