using System.Collections;

namespace Lowering.Iterators;

public partial class CustomQueue<T>
{
    public Enumerator GetEnumerator()
    {
       return new Enumerator(this); // foreach use duck typing, so return directly struct without boxing
    }
    
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)this).GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private const int Finished = -2;
        private const int NotStarted = -1;
        
        internal Enumerator(CustomQueue<T> queue)
        {
            _queue = queue;

            _version = _queue._version;
            _index = NotStarted;
            
            _current = default!;
        }
        
        private readonly CustomQueue<T> _queue;
        
        private int _index;
        private int _version;
        
        private T _current;

        public bool MoveNext()
        {
            if (_version != _queue._version)
                throw new InvalidOperationException("Collection was modified");
            
            if(_index == Finished) 
                return false;

            _index++;

            if (_index == _queue.Count)
            {
                _index = Finished;
                _current = default!;
             
                return false;
            }

            _current = _queue._array[(_queue._head + _index) % _queue.Capacity];
            return true;
        }

        void IEnumerator.Reset()
        {
            if (_version != _queue._version)
                throw new InvalidOperationException("Collection was modified");
            
            _index = NotStarted;
            _current = default!;
        }

        public T Current => _current;
        object? IEnumerator.Current => _current;

        public void Dispose()
        {
            _index = Finished;
            _current = default!;
        }
    }
}