namespace Threading.SignalPrimitives.OrderPipeline;

public class OrderPipeline : IDisposable
{
    public OrderPipeline()
    {
        _validator  = new Thread(ValidatorLoop)  { Name = "Validator"  };
        _enricher   = new Thread(EnricherLoop)   { Name = "Enricher"   };
        _dispatcher = new Thread(DispatcherLoop) { Name = "Dispatcher" };
    }
    
    private const float TimeOut = 2.0f;
    
    private readonly AutoResetEvent _validatorToEnricher  = new(false);
    private readonly AutoResetEvent _enricherToDispatcher = new(false);

    private Order? _current;
    private bool   _validationPassed;
    private string _enrichedRegion = "";

    private readonly Thread _validator;
    private readonly Thread _enricher;
    private readonly Thread _dispatcher;

    private readonly Queue<Order> _orders = new();
    private readonly object       _lock   = new();
    private volatile bool         _done;
    
    public void Submit(Order order)
    {
        lock (_lock)
        {
            _orders.Enqueue(order);
        }
    }

    public void Start()
    {
        _validator.Start();
        _enricher.Start();
        _dispatcher.Start();
    }

    private void ValidatorLoop()
    {
        while (!_done || _orders.Count > 0)
        {
            Order? order = null;
            
            lock (_lock)
            {
                if (_orders.Count > 0)
                    order = _orders.Dequeue();
            }

            if (order is null)
            {
                Thread.Sleep(50); 
                continue;
            }

            Console.WriteLine($"[Validator]  Checking order #{order.Id}");
            Thread.Sleep(200);

            _current          = order;
            _validationPassed = order.Price > 0 && !string.IsNullOrEmpty(order.Item);

            if (!_validationPassed)
                Console.WriteLine($"[Validator]  Order #{order.Id} FAILED validation");

            _validatorToEnricher.Set();
        }

        _done = true;
    }

    private void EnricherLoop()
    {
        while (true)
        {
            bool signaled = _validatorToEnricher.WaitOne(TimeSpan.FromSeconds(TimeOut));

            if (!signaled)
            {
                if (_done) 
                    break;
                
                Console.WriteLine("[Enricher]   Timeout waiting for Validator");
                continue;
            }

            if (!_validationPassed)
            {
                Console.WriteLine($"[Enricher]   Skipping failed order #{_current?.Id}");
                _enricherToDispatcher.Set();
                continue;
            }

            Console.WriteLine($"[Enricher]   Enriching order #{_current!.Id}");
            Thread.Sleep(150);
            _enrichedRegion = _current.Item.Length % 2 == 0 ? "EU" : "US";

            _enricherToDispatcher.Set();
        }
    }

    private void DispatcherLoop()
    {
        while (true)
        {
            bool signaled = _enricherToDispatcher.WaitOne(TimeSpan.FromSeconds(TimeOut));

            if (!signaled)
            {
                if (_done) 
                    break;
                
                Console.WriteLine("[Dispatcher] Timeout waiting for Enricher");
                continue;
            }

            if (!_validationPassed)
            {
                Console.WriteLine($"[Dispatcher] Dropping invalid order #{_current?.Id}\n");
                continue;
            }

            Console.WriteLine($"[Dispatcher] Dispatching order #{_current!.Id}, region: {_enrichedRegion}\n");
            Thread.Sleep(100);
        }
    }

    public void Complete()
    {
        while (_orders.Count > 0) 
            Thread.Sleep(50);
        
        Thread.Sleep(200);
        _done = true;
        
        _validator.Join();
        _enricher.Join();
        _dispatcher.Join();
    }

    public void Dispose()
    {
        _validatorToEnricher.Dispose();
        _enricherToDispatcher.Dispose();
    }
}

