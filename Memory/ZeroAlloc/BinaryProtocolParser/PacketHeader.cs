using System.Buffers.Binary;

namespace Memory.ZeroAlloc.BinaryProtocolParser;

public ref struct PacketHeader
{
    public uint Magic;
    public ushort Version;
    public int PayloadLength;

    public static bool TryRead(ReadOnlySpan<byte> source, out PacketHeader header, out ReadOnlySpan<byte> payload)
    {
        header = default;
        payload = default;

        const int headerSize = 4 + 2 + 4;
        if (source.Length < headerSize) return false;

        header.Magic          = BinaryPrimitives.ReadUInt32BigEndian(source);
        header.Version        = BinaryPrimitives.ReadUInt16BigEndian(source[4..]);
        header.PayloadLength  = BinaryPrimitives.ReadInt32BigEndian(source[6..]);

        if (source.Length < headerSize + header.PayloadLength) 
            return false;

        payload = source.Slice(headerSize, header.PayloadLength);
        
        return true;
    }
}