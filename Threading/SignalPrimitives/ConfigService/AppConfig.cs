namespace Threading.SignalPrimitives.ConfigService;

public class AppConfig
{
    public string  DatabaseUrl  { get; set; } = "db://localhost:5432";
    public int     MaxRetries   { get; set; } = 3;
    public string  Environment  { get; set; } = "production";
}