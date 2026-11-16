namespace Lowering.Examples;

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
}