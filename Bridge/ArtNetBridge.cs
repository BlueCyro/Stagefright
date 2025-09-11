using FrooxEngine;
using System.Net;
using ArtfullySimple;


namespace Stagefright;

public static class ArtNetBridge
{
    public static readonly HashSet<Universe> Universes = new();
    public static ArtNetClient recv;
    public static event EventHandler<ArtDmxPacket>? DMXRoute;
    public static readonly IPAddress ListenAddress;



    static ArtNetBridge()
    {
        ListenAddress = IPAddress.Any;
        Stagefright.Msg($"Constructing bridge and setting listener interface to {ListenAddress}");
        recv = new(ListenAddress);
    }



    public static void StartListening()
    {
        Stagefright.Msg($"Starting ArtNet client listener on {ListenAddress}!");
        recv.ReceivedPacket += ReceivePacket;
        recv.StartListening();
    }



    public static void ReceivePacket(object? sender, ArtNetPacket args)
    {
        if (args is ArtDmxPacket packet)
        {
            DMXRoute?.Invoke(sender, packet);
        }
    }



    // Extensions
    public static Universe SetupUniverse(this World world, int numChannels, int address)
    {
        Universe uni = new(world, numChannels, address);
        Universes.Add(uni);
        return uni;
    }



    // TODO: Get off my butt and make this less bad
    public static void DestroyUniverse(this World world, int address)
    {
        Universes.FirstOrDefault(uni => uni.World == world && uni.Address == address)?.Destroy();
    }



    // TODO: Get off my butt some more and also make this less bad
    public static void DestroyUniverses(this World world)
    {
        foreach (var uni in Universes.ToList())
        {
            if (uni.World == world)
                uni.Destroy();
        }
    }
}
