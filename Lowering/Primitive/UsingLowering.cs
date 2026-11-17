namespace Lowering.Primitive;

public class UsingLowering
{
    public class ClassDisposable : IDisposable
    {
        public void Dispose()
        {
            
        }
    }
    
    public void Code1()
    {
        using (ClassDisposable classDisposable = new ClassDisposable())
        {
            Console.WriteLine("class work");
        }
    }

    public void Lowering1()
    {
        ClassDisposable classDisposable = new ClassDisposable();
        try
        {
            Console.WriteLine("class work");
        }
        finally
        {
            if (classDisposable != null)
            {
                ((IDisposable)classDisposable).Dispose();
            }
        }
    }
    
    public struct StructDisposable : IDisposable
    {
        public int X, Y;
        public void Dispose()
        {
            
        }
    }
    
    public void Code2()
    {
        using (StructDisposable structDisposable = new StructDisposable())
        {
            Console.WriteLine("struct work");
        }
    }

    public void Lowering2()
    {
        StructDisposable structDisposable = default(StructDisposable);
        try
        {
            Console.WriteLine("struct work");
        }
        finally
        {
            ((IDisposable)structDisposable).Dispose();
        }
    }
    
    public void Code3()
    {
        var s1 = new StructDisposable();
        using (s1)
        {
            Console.WriteLine("before disposing");
            s1.X++; // using fields 
        }
        
        Console.WriteLine("after disposing");
    }
    public void Lowering3()
    {
        StructDisposable s1 = default(StructDisposable);
        StructDisposable s2 = s1; // Copy (because of using)
        try
        {
            Console.WriteLine("before disposing");
            s1.X++; // Changing the s1 when s2 will dispose 
        }
        finally
        {
            ((IDisposable)s2).Dispose();
        }
        Console.WriteLine("after disposing");
    }
    
    
    public ref struct RefStructDisposable
    {
        public int X;
        public void Dispose()
        {
            
        }
    }
    public void Code4()
    {
        var s1 = new RefStructDisposable();
        using (s1)
        {
            Console.WriteLine("before disposing");
            s1.X++; // using fields 
        }
        
        Console.WriteLine("after disposing");
    }

    public void Lowering4()
    {
        RefStructDisposable s1 = default(RefStructDisposable);
        RefStructDisposable s2 = s1; // Copy (because of using)
        try
        {
            Console.WriteLine("before disposing");
            s1.X++; // Changing the s1 when s2 will dispose 
        }
        finally
        {
            s2.Dispose(); // for ref structures duck typing
        }
        Console.WriteLine("after disposing");
    }
}