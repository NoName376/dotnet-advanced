using System.Buffers;

namespace Memory.ZeroAlloc.ZeroAllocLogger;

public ref struct PooledBuffer : IMemoryOwner<char>
{
    public PooledBuffer(int minLength)
    {
        _array  = ArrayPool<char>.Shared.Rent(minLength);
        _length = minLength;
        Memory  = new Memory<char>(_array, 0, _length);
    }
    
    public Memory<char> Memory { get; }

    public void Dispose()
    {
        if (_array is null) 
            return;
        
        ArrayPool<char>.Shared.Return(_array);
        
        _array = null;
    }
    
        
    private char[]? _array;
    private readonly int _length;
}