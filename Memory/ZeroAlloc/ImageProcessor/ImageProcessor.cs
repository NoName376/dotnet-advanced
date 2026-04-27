using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Memory.ZeroAlloc.ImageProcessor;

public sealed class ImageProcessor
{
    public ImageProcessor(int width, int height)
    {
        _width  = width;
        _height = height;
        _buffer = new byte[width * height * 3];
        
        Fill();
    }
    
    private void Fill()
    {
        var rng  = new Random(67);
        var span = _buffer.Span;
        
        for (int i = 0; i < span.Length; i++)
            span[i] = (byte) rng.Next(256);
    }

    private Span<Rgb24> Pixels =>
        MemoryMarshal.Cast<byte, Rgb24>(_buffer.Span);

    public void Grayscale()
    {
        var pixels = Pixels;
        
        for (int i = 0; i < pixels.Length; i++)
        {
            ref var p   = ref pixels[i];
            byte gray         = (byte)(p.R * 0.299f + p.G * 0.587f + p.B * 0.114f);
            p.R = p.G = p.B   = gray;
        }
    }

    public unsafe void BrightenSIMD(float factor)
    {
        var span  = _buffer.Span;
        int total = span.Length;
        int i     = 0;

        fixed (byte* ptr = span)
        {
            if (Vector128.IsHardwareAccelerated)
            {
                int simdWidth = Vector128<byte>.Count;
                byte sat      = (byte)Math.Clamp((int)(factor * 64), 0, 255);
                var addVec    = Vector128.Create(sat);

                for (; i <= total - simdWidth; i += simdWidth)
                {
                    var v      = Vector128.Load(ptr + i);
                    var result = Vector128.Min(
                        Vector128.Create((byte)255),
                        v + addVec
                    );
                    result.Store(ptr + i);
                }
            }

            for (; i < total; i++)
                ptr[i] = (byte)Math.Min(255, ptr[i] + (int)(factor * 64));
        }
    }
    public unsafe void BrightenNaive(float factor)
    {
        var span = _buffer.Span;
        fixed (byte* ptr = span)
            for (int i = 0; i < span.Length; i++)
                ptr[i] = (byte)Math.Min(255, ptr[i] + (int)(factor * 64));
    }
    public unsafe void FlipHorizontal()
    {
        fixed (byte* ptr = _buffer.Span)
        {
            var pixels = (Rgb24*)ptr;
            for (int y = 0; y < _height; y++)
            {
                Rgb24* row = pixels + y * _width;
                int left   = 0;
                int right  = _width - 1;
                
                while (left < right)
                {
                    Rgb24 tmp       = row[left];
                    row[left]       = row[right];
                    row[right]      = tmp;
                    
                    left++;
                    right--;
                }
            }
        }
    }

    private readonly Memory<byte> _buffer;
    private readonly int _width;
    private readonly int _height;
    
    public void Benchmark()
    {
        const int iterations = 20;
        long allocBefore, allocAfter;

        Console.WriteLine($"\n Image: {_width}x{_height} ({_buffer.Length / 1024 / 1024} MB)");

        Console.WriteLine("\nBrighten SIMD");
        allocBefore = GC.GetTotalMemory(true);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) BrightenSIMD(1.2f);
        sw.Stop();
        allocAfter = GC.GetTotalMemory(false);
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms;  Alloc: {allocAfter - allocBefore} bytes");

        
        
        Console.WriteLine("\nBrighten Naive");
        allocBefore = GC.GetTotalMemory(true);
        sw.Restart();
        for (int i = 0; i < iterations; i++) BrightenNaive(1.2f);
        sw.Stop();
        allocAfter = GC.GetTotalMemory(false);
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms;  Alloc: {allocAfter - allocBefore} bytes");

        Console.WriteLine("\nGrayscale");
        sw.Restart();
        for (int i = 0; i < iterations; i++) Grayscale();
        sw.Stop();
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");

        Console.WriteLine("\nFlipHorizontal");
        sw.Restart();
        for (int i = 0; i < iterations; i++) FlipHorizontal();
        sw.Stop();
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
    }
}