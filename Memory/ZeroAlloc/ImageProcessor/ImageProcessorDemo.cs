namespace Memory.ZeroAlloc.ImageProcessor;

public static class ImageProcessorDemo
{
    public static void Run()
    {
        var processor = new ImageProcessor(3840, 2160);

        processor.Grayscale();
        processor.BrightenSIMD(1.5f);
        processor.FlipHorizontal();

        processor.Benchmark();
    }
}