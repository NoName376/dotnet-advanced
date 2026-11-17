namespace Lowering.Primitive;

public class LockLowering
{
    private object _syncRoot = new(); 
    private Lock _sync = new();
    
    public void Code1()
    {
        lock (_syncRoot)
        {
            Console.WriteLine("Lock");
        }
    }

    public void Lowering1()
    {
        var lockObj = _syncRoot;
        bool lockTaken = false;

        try
        {
            lockTaken = true;
            Monitor.Enter(lockObj, ref lockTaken);
            
            Console.WriteLine("Lock");
        }
        finally
        {
            if(lockTaken)
                Monitor.Exit(lockObj);
        }
    }

    public void Code2()
    {
        lock (_sync)
        {
            Console.WriteLine("Lock");
        }
    }

    public void Lowering2()
    {
        using (Lock.Scope sc = _sync.EnterScope())
        {
            Console.WriteLine("Lock");
        }
    }
}