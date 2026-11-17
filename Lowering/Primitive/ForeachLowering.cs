using System.Collections;

namespace Lowering.Primitive;

public class ForeachLowering
{
    public void Code1()
    {
        int[] nums = [1, 2, 3, 4];
        foreach (int item in nums)
        {
            Console.WriteLine(item);
        }
    }

    public void Lowering1()
    {
        int[] nums = [1, 2, 3, 4];
        for (int i = 0; i < nums.Length; i++)
        {
            Console.WriteLine(nums[i]);
        }
    }

    public void Code2()
    {
        List<int> nums = [1, 2, 3, 4];
        foreach (int item in nums)
        {
            Console.WriteLine(item);
        }
    }

    public void Lowering2()
    {
        List<int> nums = [1, 2, 3, 4];
        List<int>.Enumerator enumerator = nums.GetEnumerator(); // LIST.ENUMERATOR
        try
        {
            while (enumerator.MoveNext()) // List check versions
            {
                int current = enumerator.Current;
                Console.WriteLine(current);
            }
        }
        finally
        {
            ((IDisposable)enumerator)?.Dispose();
        }
    }

    public void Code3()
    {
        IEnumerable<int> nums = [1, 2, 3, 4];
        foreach (int item in nums)
        {
            Console.WriteLine(item);
        }
    }

    public void Lowering3()
    {
        IEnumerable<int> nums = [1, 2, 3, 4];
        IEnumerator<int> enumerator = nums.GetEnumerator();

        try
        {
            while (enumerator.MoveNext())
            {
                int current = enumerator.Current;

                Console.WriteLine(current);
            }
        }
        finally
        {
            ((IDisposable)enumerator)?.Dispose();
        }
    }

    public struct Foo // without interface
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    
    public void Code4()
    {
        foreach (int item in new Foo())
        {
            Console.WriteLine(item);
        }
    }

    public void Lowering4()
    {
        Foo foo = default(Foo);
        IEnumerator enumerator = foo.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                int value = (int)enumerator.Current;
                
                Console.WriteLine(value);
            }
        }
        finally
        {
            ((IDisposable)enumerator)?.Dispose();
        }
    }


    public void DistinctVarsForeach()
    {
        var actions = new List<Action>();
        
        foreach (var x in new[] { 1, 2, 3 })
            actions.Add(() => Console.WriteLine(x));

        foreach (var a in actions) 
            a(); // 1, 2, 3
    }

    public void SameVarsFor()
    {
        var actions = new List<Action>();
        
        for (int i = 0; i < 3; i++)
            actions.Add(() => Console.WriteLine(i));

        foreach (var a in actions) 
            a(); // 3, 3, 3
    }
}