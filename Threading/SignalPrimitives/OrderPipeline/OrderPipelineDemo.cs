namespace Threading.SignalPrimitives.OrderPipeline;

public static class OrderPipelineDemo
{
    public static void Run()
    {
        using var pipeline = new OrderPipeline();

        pipeline.Submit(new Order(1, "Laptop",  999.99m));
        pipeline.Submit(new Order(2, "",        50.00m));
        pipeline.Submit(new Order(3, "Monitor", 399.00m));
        pipeline.Submit(new Order(4, "Mouse",   -1.00m));
        pipeline.Submit(new Order(5, "Keyboard",79.00m));

        pipeline.Start();
        pipeline.Complete();
    }
}