
namespace Stagefright;

// Future-proofing the enum even though it's not used very much yet
public enum ArtOpCode
{
    OpPoll = 0x2000,
    OpPollReply = 0x2100,
    OpAddress = 0x6000,
    OpInput = 0x7000,
    OpIpProg = 0xF800,
    OpIpProgReply = 0xF900,
    OpCommand = 0x2400,
    OpDmx = 0x5000,
    OpNzs = 0x5100,
    OpSync = 0x5200,
}
