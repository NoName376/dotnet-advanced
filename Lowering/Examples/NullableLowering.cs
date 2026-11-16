namespace Lowering.Examples;

public class NullableLowering
{

    public void Code1()
    {
        int? nullableInt = null;
        string? nullableString = null;
        
        nullableInt = nullableInt ?? 5;
        nullableInt ??= 5;

        Console.WriteLine(nullableInt?.GetHashCode());
    }
    public void Lowering1()
    {
        Nullable<int> nullableInt = null;
        string nullableString = null;
        
        nullableInt = nullableInt.GetValueOrDefault(5);
        
        nullableInt = nullableInt.GetValueOrDefault(5);

        Console.WriteLine(nullableInt.HasValue ? nullableInt.GetHashCode() : null);
    }

    public void Code2()
    {
        int[] arr = null;
        int? first = arr?[0];
    }

    public void Lowering2()
    {
        int[] arr = null;
        
        Nullable<int> first = arr == null ? null : new Nullable<int>(arr[0]);
    }

    public void Code3()
    {
        string foo = null;
        string result = foo ?? throw new NullReferenceException();
    }

    public void Lowering3()
    {
        string foo = null;
        string result = foo == null ? throw new NullReferenceException() : foo;
    }


    public void Code4()
    {
        long? name = 23;
        
        Console.WriteLine(name!);
    }

    public void Lowering4()
    {
        Nullable<long> name = 23;
        
        Console.WriteLine(name!);
    }
}