
namespace Stagefright;

public struct ArtNetMessage(byte[] data)
{
    public const int OP_DMX = 0x5000;
    public readonly bool IsDMX => OpCode == OP_DMX;
    public readonly int OpCode => (data[9] << 8) | data[8]; // Account for little-endian
    public readonly int Universe => ((data[15] << 8) | data[14]) + 1; // Yet again account for little-endian, and also the fact that Universes are 1-indexed >:(
    public readonly byte Physical => data[13];
    public readonly byte[] PacketData => data;
    public readonly byte ProtocolHi => data[10];
    public readonly byte ProtocolLo => data[11];
    public readonly byte Sequence => data[12];
    public readonly byte LengthHi => data[16];
    public readonly byte LengthLo => data[17];
    public readonly Span<byte> DMXData => PacketData.AsSpan(18); // Present as span for easy slicing
}
