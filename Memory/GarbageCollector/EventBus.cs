using System.Collections.Concurrent;

namespace Memory.GarbageCollector;

public static class EventBusRunner
{
    public static void Run()
    {
        var eventBus = new EventBus();
        var (eventHandler, eventHandler2) = (new EventHandler(), new EventHandler2());
        
        eventBus.Subscribe(eventHandler);
        
        eventBus.Publish(new Event(42));
        eventBus.Publish(new Event(50));
        
        eventBus.Subscribe(eventHandler2);
        eventBus.Publish(new Event(100));
        
        // not necessarily (because of WeakRef)
        if (false)
        {
            eventBus.Unsubscribe(eventHandler);
            eventBus.Unsubscribe(eventHandler2);
        }
    }

    private record Event(int? Arg) : IEvent;

    private record EventHandler2 : IEventHandler<Event>
    {
        public void Handle(Event? evt)
        {
            Console.WriteLine($"[{nameof(EventHandler2)}] Event received: {evt?.Arg}");
        }
    }
    private record EventHandler : IEventHandler<Event>
    {
        public void Handle(Event? evt)
        {
            Console.WriteLine($"[{nameof(EventHandler)}] Event received: {evt?.Arg}");
        }
    }
}


public sealed class EventBus
{
    private readonly ConcurrentDictionary<Type, HandlerBucket> _handlers = new();

    public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var type = typeof(T);
        var bucket = _handlers.GetOrAdd(type, _ => new HandlerBucket());

        bucket.Add(handler);
    }

    public void Publish<T>(T? evt) where T : IEvent
    {
        if (evt == null) return;

        if (!_handlers.TryGetValue(typeof(T), out var bucket))
            return;

        bucket.Invoke(evt);
    }

    public void Unsubscribe<T>(IEventHandler<T>? handler) where T : IEvent
    {
        if (handler == null) return;

        if (_handlers.TryGetValue(typeof(T), out var bucket))
        {
            bucket.Remove(handler);
        }
    }
    
    private sealed class HandlerBucket
    {
        private WeakHandler[] _handlers = Array.Empty<WeakHandler>();

        public void Add<T>(IEventHandler<T> handler) where T : IEvent
        {
            var weak = new WeakHandler(handler);

            while (true)
            {
                var snapshot = _handlers;
                var newArr = new WeakHandler[snapshot.Length + 1];

                Array.Copy(snapshot, newArr, snapshot.Length);
                newArr[^1] = weak;

                if (Interlocked.CompareExchange(ref _handlers, newArr, snapshot) == snapshot)
                    return;
            }
        }

        public void Remove<T>(IEventHandler<T>? handler) where T : IEvent
        {
            while (true)
            {
                var snapshot = _handlers;
                int count = snapshot.Length;

                int index = -1;
                for (int i = 0; i < count; i++)
                {
                    if (snapshot[i].IsAlive && ReferenceEquals(snapshot[i].Target, handler))
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                    return;

                var newArr = new WeakHandler[count - 1];

                if (index > 0)
                    Array.Copy(snapshot, 0, newArr, 0, index);

                if (index < count - 1)
                    Array.Copy(snapshot, index + 1, newArr, index, count - index - 1);

                if (Interlocked.CompareExchange(ref _handlers, newArr, snapshot) == snapshot)
                    return;
            }
        }

        public void Invoke<T>(T evt) where T : IEvent
        {
            var snapshot = _handlers;

            for (int i = 0; i < snapshot.Length; i++)
            {
                var h = snapshot[i];

                if (!h.IsAlive)
                    continue;

                if (h.Target is IEventHandler<T> typed)
                {
                    typed.Handle(evt);
                }
            }

            CleanupDead(snapshot);
        }

        private void CleanupDead(WeakHandler[] snapshot)
        {
            int aliveCount = 0;

            for (int i = 0; i < snapshot.Length; i++)
                if (snapshot[i].IsAlive)
                    aliveCount++;

            if (aliveCount == snapshot.Length)
                return;

            var newArr = new WeakHandler[aliveCount];
            int j = 0;

            for (int i = 0; i < snapshot.Length; i++)
            {
                var h = snapshot[i];
                if (h.IsAlive)
                    newArr[j++] = h;
            }

            Interlocked.CompareExchange(ref _handlers, newArr, snapshot);
        }
    }
    private readonly struct WeakHandler
    {
        public WeakHandler(object target)
        {
            _ref = new WeakReference(target);
        }
        
        private readonly WeakReference _ref;

        public object Target => _ref.Target!;
        public bool IsAlive => _ref.IsAlive;
    }
}

public interface IEventHandler<T> where T : IEvent
{
    void Handle(T evt);
}
public interface IEvent { }