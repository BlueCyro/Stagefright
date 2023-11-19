using System.Text;
namespace Stagefright;

// Currently only supporting the ingest of DMX data, will build this out as-required in the future.
public struct ArtNetMessage(byte[] data)
{
    public static string ART_ID = Encoding.ASCII.GetString( new byte[] { 65, 114, 116, 45, 78, 101, 116, 0 } );
    
    public readonly bool IsValid => ID == ART_ID && Protocol >= 14; // Make sure the payload is actually ArtNet and do the 'good version' check
    public readonly string ID => Encoding.ASCII.GetString(data, 0, 8);
    public readonly ArtOpCode Op => (ArtOpCode)((data[9] << 8) | data[8]); // Lower byte first
    public readonly short Protocol => (short)((data[10] << 8) | data[11]); // Higher byte first
    public readonly byte Sequence => data[12];
    public readonly int Universe => ((data[15] << 8) | data[14]) + 1; // Yet again account for little-endian, and also the fact that Universes are 1-indexed >:(
    public readonly byte Physical => data[13];
    public readonly byte[] PacketData => data;
    public readonly byte LengthHi => data[16];
    public readonly byte LengthLo => data[17];
    public readonly Span<byte> DMXData => data.AsSpan(18); // Present as span for easy slicing
}
