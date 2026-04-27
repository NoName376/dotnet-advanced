using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;

namespace Memory.ZeroAlloc.BinaryProtocolParser;

public static class PacketProcessor
{
    public static void Process(ReadOnlySpan<byte> buffer, bool isPrint = true)
    {
        const int headerSize = 4 + 2 + 4;
        int offset = 0;
        int packetCount = 0;

        while (offset < buffer.Length)
        {
            var window = buffer[offset..];
            if (!PacketHeader.TryRead(window, out var header, out var payload))
                break;

            if(isPrint)
                Console.WriteLine($"Packet #{++packetCount}  Magic: 0x{header.Magic:X8}  Ver: {header.Version}  Payload: {payload.Length} bytes");

            offset += headerSize + header.PayloadLength;
        }
    }
    private static void NaiveParse(byte[] buffer, bool isPrint = true)
    {
        const int headerSize = 10;
        int offset = 0;
        int packetCount = 0;
        
        while (offset < buffer.Length)
        {
            using var ms = new MemoryStream(buffer, offset, buffer.Length - offset);
            using var br = new BinaryReader(ms);
            if (buffer.Length - offset < headerSize) break;
            uint magic     = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
            ushort version = (ushort)IPAddress.NetworkToHostOrder(br.ReadInt16());
            int length     = IPAddress.NetworkToHostOrder(br.ReadInt32());
            if (offset + headerSize + length > buffer.Length) break;
            var payload    = br.ReadBytes(length);
            
            if(isPrint)
                Console.WriteLine($"Packet #{++packetCount}  Magic: 0x{magic:X8}  Ver: {version}  Payload: {payload.Length} bytes");

            
            offset += headerSize + length;
        }
    }
    
    
    
    public static void BenchmarkVsNaive(byte[] buffer, int iterations)
    {
        long memBefore, memAfter;

        Console.WriteLine("\nSpan-based parser");
        memBefore = GC.GetTotalMemory(true);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            Process(buffer, false);
        sw.Stop();
        memAfter = GC.GetTotalMemory(false);
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms;  Alloc delta: {memAfter - memBefore} bytes");

        Console.WriteLine("\nNaive BinaryReader parser");
        memBefore = GC.GetTotalMemory(true);
        sw.Restart();
        for (int i = 0; i < iterations; i++)
            NaiveParse(buffer, false);
        sw.Stop();
        memAfter = GC.GetTotalMemory(false);
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms;  Alloc delta: {memAfter - memBefore} bytes");
    }
    public static byte[] BuildTestBuffer(int packetCount)
    {
        const int headerSize = 10;
        const int payloadSize = 32;
        var buf = new byte[packetCount * (headerSize + payloadSize)];
        var span = buf.AsSpan();
        int offset = 0;

        for (int i = 0; i < packetCount; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(span[offset..],       0xDEADBEEF);
            BinaryPrimitives.WriteUInt16BigEndian(span[(offset + 4)..], 1);
            BinaryPrimitives.WriteInt32BigEndian(span[(offset + 6)..],  payloadSize);
            span.Slice(offset + headerSize, payloadSize).Fill((byte)(i & 0xFF));
            offset += headerSize + payloadSize;
        }
        return buf;
    }
}