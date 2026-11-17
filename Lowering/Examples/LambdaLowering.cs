namespace Lowering.Examples;

public class LambdaLowering
{
    public delegate int Op(int x, int y);

    public void Code1()
    {
        Op op = (x, y) => x - y;
    }

    public void Lowering1()
    {
        Op op = Lambda_Lowering1.Cache ?? (Lambda_Lowering1.Cache =
            Lambda_Lowering1.Singleton.Invoke);
    }

    private sealed class Lambda_Lowering1
    {
        public static Op Cache;
        public static readonly Lambda_Lowering1 Singleton = new();

        internal int Invoke(int x, int y)
        {
            return x - y;
        }
    }


    public void Code2()
    {
        Action act = () => { Console.WriteLine("Hello"); };
    }

    public void Lowering2()
    {
        Action act = Lambda_Lowering2.Cache ?? (Lambda_Lowering2.Cache =
            new Action(Lambda_Lowering2.Singleton.Invoke));
    }

    private sealed class Lambda_Lowering2
    {
        public static Action Cache;
        public static readonly Lambda_Lowering2 Singleton = new();

        public void Invoke()
        {
           Console.WriteLine("Hello");
        }
    }
    
    public void Code3()
    {
        string hello = "Hello";
        Action act = () => { Console.WriteLine(hello); };
    }

    public void Lowering3()
    {
        var closureClass = new Closure_Lowering3();
        closureClass.Hello = "Hello";

        Action act = new Action(closureClass.Invoke);
    }

    private sealed class Closure_Lowering3
    {
        // No cache and singleton
        
        public string Hello;

        public void Invoke()
        {
            Console.WriteLine(Hello);
        }
    }
    
    public void Code4()
    {
        var actions = new List<Action>();
        for (int i = 0; i < 3; i++)
        {
            actions.Add(() => Console.WriteLine(i));
        }
        foreach (var a in actions) a(); // 3 3 3
    }

    public void Lowering4()
    {
        var actions = new List<Action>();
        Closure_Lowering4 closure = new Closure_Lowering4();
        closure.i = 0;
        while (closure.i < 3)
        {
            actions.Add(new Action(closure.Invoke));
            closure.i++;
        }
        foreach (var a in actions) 
            a();
    }
    private sealed class Closure_Lowering4
    {
        public int i;

        internal void Invoke()
        {
            Console.WriteLine(i);
        }
    }
    
    public void Code5()
    {
        var actions = new List<Action>();
        for (int i = 0; i < 3; i++)
        {
            int copy = i;
            actions.Add(() => Console.WriteLine(copy));
        }
        foreach (var a in actions) 
            a(); // 0 1 2
    }

    public void Lowering5()
    {
        var actions = new List<Action>();
        int i = 0;
        while (i < 3)
        {
            Closure_Lowering5 closure = new Closure_Lowering5();
            closure.copy = i;
            actions.Add(new Action(closure.Invoke));
            i++;
        }
        foreach (var a in actions) 
            a();
    }

    private sealed class Closure_Lowering5
    {
        public int copy;

        internal void Invoke()
        {
            Console.WriteLine(copy);
        }
    }


    public void Code6()
    {
        int a = 10;
        int b = 20;

        Action action1 = () => Console.WriteLine(a);
        Action action2 = () => Console.WriteLine(b);
        Action action3 = () => Console.WriteLine(a + b);

        action1();
        action2();
        action3();
    }

    public void Lowering6()
    {
        var closure = new Closure_Lowering6();

        closure.a = 10;
        closure.b = 20;

        Action action1 = new Action(closure.Action1);
        Action action2 = new Action(closure.Action2);
        Action action3 = new Action(closure.Action3);

        action1();
        action2();
        action3();
    }

    private sealed class Closure_Lowering6 // One class for scope
    {
        public int a, b;
        
        internal void Action1()
        {
            Console.WriteLine(a);
        }

        internal void Action2()
        {
            Console.WriteLine(b);
        }

        internal void Action3()
        {
            Console.WriteLine(a + b);
        }
    }

    private static int x = 10;
    public void Code7()
    {
        const int y = 10; 
        int uncaptured = 345;
        
        Action act = static () => { Console.WriteLine(x + y); }; // static lambda prohibit nonstatic - closure
        
        Console.WriteLine(act.Target); // Closure_Lowering7 class
    }

    public void Lowering7()
    {
        int uncaptured = 345;

        Action act = Closure_Lowering7.Cache ?? (Closure_Lowering7.Cache =
            Closure_Lowering7.Singleton.Invoke);
        
        Console.WriteLine(act.Target); // Closure_Lowering7 class
    }

    private sealed class Closure_Lowering7
    {
        public static Action Cache;
        public static readonly Closure_Lowering7 Singleton = new();

        internal void Invoke()
        {
            Console.WriteLine(x + 10); // Const are inline 
        }
    }
}