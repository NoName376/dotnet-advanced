namespace Lowering.Iterators;

public partial class CustomQueue<T> : IEnumerable<T>
{
    public CustomQueue(IEnumerable<T>? collection)
    {
        _array = collection?.ToArray() ?? Array.Empty<T>();
        Count = _array.Length;
    }
    public CustomQueue() : this(Array.Empty<T>())
    { }

    public void Enqueue(T item)
    {
        if (Capacity == Count)
            Grow();

        int tail = (_head + Count) % Capacity;
        _array[tail] = item;
        
        Count++;
        _version++;
    }

    public T Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty");
        
        T result = _array[_head];
        
        _array[_head] = default!;
        
        _head = (_head + 1) % Capacity;
        Count--;
        _version++;

        return result;
    }

    public bool TryDequeue(out T item)
    {
        if (Count == 0)
        {
            item = default!;
            return false;
        }
        else
        {
            item = Dequeue();
            return true;
        }
    }

    public T Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty");
    
        return _array[_head];
    }

    public void Clear()
    {
        Array.Clear(_array, 0, _array.Length);
        
        _head = 0;
        Count = 0;
        _version++;
    }
    
    private void Grow()
    {
        var newArray = new T[Math.Max(Capacity * 2, Count + 1)];
        
        if (Count > 0)
        {
            if (_head + Count <= Capacity)
            {
                Array.Copy(_array, _head, newArray, 0, Count);
            }
            else
            {
                int firstPart = Capacity - _head;
                
                Array.Copy(_array, _head, newArray, 0, firstPart);
                Array.Copy(_array, 0, newArray, firstPart, Count - firstPart);
            }
        }
        
        _array = newArray;
        _head = 0;
    }
    
    
    public int Capacity => _array.Length;
    public int Count { get; private set; }
    private T[] _array;

    private int _version;
    private int _head;
}