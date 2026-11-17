namespace Lowering.Primitive;

public class EventsLowering
{
    #region Code1

    public event Action<float>? Damaged;

    #endregion

    #region Lower1
    
    private Action<float>? _damaged1;
    public event Action<float>? Damaged1
    {
        add
        {
            Action<float>? current = _damaged1;
            Action<float>? prev;

            do
            {
                prev = current;
                var combined = (Action<float>?)Delegate.Combine(prev, value);
                current = Interlocked.CompareExchange(ref _damaged1, combined, prev);
            }
            while (!ReferenceEquals(current, prev));
        }
        remove
        {
            Action<float>? current = _damaged1;
            Action<float>? prev;

            do
            {
                prev = current;
                var removed = (Action<float>?)Delegate.Remove(prev, value);
                current = Interlocked.CompareExchange(ref _damaged1, removed, prev);
            }
            while (!ReferenceEquals(current, prev));
        }
    }
    
    #endregion
}