namespace Lowering.Primitive;

public class TuplesLowering
{
    public void Code1()
    {
        var tuple = (42, "Hello");
    }

    public void Lowering1()
    {
        ValueTuple<int, string> valueTuple = new ValueTuple<int, string>(42, "Hello");
    }
    public void Code2()
    {
        var tuple = (1, 1, age:1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
    }

    public void Lowering2()
    {
        ValueTuple<int, int, int, int, int, char, int, ValueTuple<int, int, int, string, int>> valueTuple = new ValueTuple<int, int, int, int, int, char, int, ValueTuple<int, int, int, string, int>>(1, 1, 1, 1, 1, 'W', 1, new ValueTuple<int, int, int, string, int>(1, 1, 1, "Alex", 1));
    }

    public void Code3()
    {
        (string, int) tuple = ("Hello", 42);
        (string name, int age) = tuple;
    }
    
    public void Lowering3()
    {
        ValueTuple<string, int> valueTuple = new ValueTuple<string, int>("Hello", 42);
        
        string item = valueTuple.Item1;
        int item2 = valueTuple.Item2;
    }
    
    public void Code4()
    {
        var p1 = new Person();

        var (name, age) = p1;
    }
    public void Lowering4()
    {
        var p1 = new Person();
        p1.Deconstruct(out string name, out int age);
    }
    
    public void Code5()
    {
        int a = 4, b = 10;
        (b, a) = (a, b);
    }

    public void Lowering5()
    {
        int a = 4;
        int b = 10;
        
        int t = b;
        b = a;
        a = t;
    }



    public class Person
    {
        public string FirstName { get; set; }
        public int Age { get; set; }

        public string LastName { get; set; }
        
        public void Deconstruct(out string name, out int age)
        {
            name = string.Concat(FirstName, " ", LastName);
            age = Age;
        }
    }
}