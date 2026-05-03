using System.Collections.Concurrent;

namespace Threading.ThreadPool;

public static class Task2Runner
{
    public static void Run()
    {   
        var queue = new Task2.TaskQueue();

        var producer = new Task2.Producer(queue);
        var (consumer1, consumer2) = (new Task2.Consumer(queue, 1), new Task2.Consumer(queue, 2));
        
        producer.Start();
        consumer1.Start();
        consumer2.Start();

        producer.Join();
        consumer1.Finish();
        consumer2.Finish();
        

        Console.WriteLine("Producer finished!");
    }
}

public class Task2
{
    public class Producer
    {
        public Producer(TaskQueue queue)
        {
            _queue = queue;
            
            _thread = new Thread(Run);
        }
        
        private readonly TaskQueue _queue;
        private readonly Thread _thread;
        
        public void Start() => _thread.Start();

        private void Run()
        {
            for (int i = 1; i <= 20; i++)
            {
                _queue.Enqueue(i);
                Console.WriteLine($"[Producer] Added: {i}");

                Thread.Sleep(100);
            }
        }

        public void Join()
        {
            _thread.Join();
        }
    }
    public class Consumer
    {
        public Consumer(TaskQueue queue, int id)
        {
            _queue = queue;
            _id = id;
            _running = true;

            _thread = new Thread(Run);
        }
        
        private readonly TaskQueue _queue;
        private readonly int _id;
        private readonly Thread _thread;
        
        private volatile bool _running;
        
        public void Start() => _thread.Start();

        private void Run()
        {
            while (_running)
            {
                if (_queue.TryDequeue(out int item))
                {
                    Console.WriteLine($"[Consumer {_id}] Processed: {item}");
                    Thread.Sleep(150);
                }
                else
                {
                    Console.WriteLine($"[Consumer {_id}] Do nothing");
                    Thread.Sleep(50);
                }
            }
        }
        
        public void Finish()
        {
            _running = false;
        }
    }
    public class TaskQueue
    {
        private readonly ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();

        public void Enqueue(int item)
        {
            _queue.Enqueue(item);
        }

        public bool TryDequeue(out int item)
        {
            return _queue.TryDequeue(out item);
        }

        public bool IsEmpty => _queue.IsEmpty;
    }
}