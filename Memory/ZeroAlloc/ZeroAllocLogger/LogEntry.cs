namespace Memory.ZeroAlloc.ZeroAllocLogger;

public ref struct LogEntry
{
    public Severity Severity;
    public ReadOnlySpan<char> Message;
    public DateTime Timestamp;
}