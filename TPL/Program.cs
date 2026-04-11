using TPL.ThreadPool;

namespace TPL;

class Program
{
    static void Main(string[] args)
    {
        ThreadPool.ThreadPool.ThreadPoolRun.Run();
    }
}