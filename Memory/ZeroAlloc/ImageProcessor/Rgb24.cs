using System.Runtime.InteropServices;

namespace Memory.ZeroAlloc.ImageProcessor;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Rgb24
{
    public byte R, G, B;
}