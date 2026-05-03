namespace Threading.SignalPrimitives.MapReduce;

public class MapReduce : IDisposable
{
    public MapReduce(int mappers, int dataSize)
    {
        _countdown      = new CountdownEvent(mappers);
        
        _mappers        = mappers;
        _data           = Enumerable.Range(1, dataSize).ToArray();
        _partialResults = new int[mappers];
        _completed      = new bool[mappers];
    }
    
    private readonly CountdownEvent    _countdown;
    
    private readonly int      _mappers;
    private readonly int[]    _data;
    private readonly int[]    _partialResults;
    private readonly bool[]   _completed;
    
    public void RunPhases(int phases)
    {
        for (int phase = 0; phase < phases; phase++)
        {
            Console.WriteLine($"\nPhase #{phase + 1}");
            RunPhase(phase);
        }
    }

    private void RunPhase(int phase)
    {
        Array.Clear(_completed);
        _countdown.Reset(_mappers);

        using var cts        = new CancellationTokenSource();
        int chunkSize        = _data.Length / _mappers;
        var mapperThreads    = new Thread[_mappers];

        for (int i = 0; i < _mappers; i++)
        {
            var mapperId = i;
            var start    = mapperId * chunkSize;
            var end      = mapperId == _mappers - 1 ? _data.Length : start + chunkSize;

            mapperThreads[i] = new Thread(() =>
            {
                bool shouldFail = phase == 1 && mapperId == 2;
                
                Console.WriteLine($"[Mapper {mapperId}] Processing chunk [{start}..{end}), phase {phase + 1}");
                Thread.Sleep(Random.Shared.Next(100, 400));

                if (shouldFail)
                {
                    Console.WriteLine($"[Mapper {mapperId}] CRASHED on phase {phase + 1}");
                    cts.CancelAfter(300);
                    _countdown.Signal();
                    return;
                }

                int sum = 0; for (int j = start; j < end; j++)
                    sum += _data[j] * (phase + 1);

                _partialResults[mapperId] = sum;
                _completed[mapperId]      = true;

                Console.WriteLine($"[Mapper {mapperId}] Partial sum: {sum}");
                _countdown.Signal();

            }) { Name = $"Mapper-{mapperId}-P{phase}" };
        }

        foreach (var t in mapperThreads) t.Start();

        bool allDone = _countdown.Wait(TimeSpan.FromSeconds(3), 
            cts.Token == default
            ? CancellationToken.None
            : cts.Token);

        Reduce(phase, allDone);

        foreach (var t in mapperThreads)
            t.Join();
    }

    private void Reduce(int phase, bool allDone)
    {
        Console.WriteLine($"\n[Reducer] Aggregating phase {phase + 1} (complete={allDone})");

        long total        = 0;
        var  missing      = new List<int>();

        for (int i = 0; i < _mappers; i++)
        {
            if (_completed[i])
                total += _partialResults[i];
            else
                missing.Add(i);
        }

        if (missing.Count > 0)
            Console.WriteLine($"[Reducer] Missing chunks from mappers: [{string.Join(", ", missing)}]");

        Console.WriteLine($"[Reducer] Phase {phase + 1} result: {total} (from {_mappers - missing.Count}/{_mappers} mappers)");
    }

    public void Dispose() => _countdown.Dispose();
}