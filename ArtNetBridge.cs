using FrooxEngine;
using System.Net;
using ArtfullySimple;

namespace Stagefright;

public static class ArtNetBridge
{
    public static Dictionary<World, ArtNetRouter> routers = new();
    public static ArtNetClient recv = new(IPAddress.Any);

    public static void StartListening()
    {
        Stagefright.Msg($"Starting ArtNet client listener on: {IPAddress.Any}");
        recv.ReceivedPacket += ReceivePacket;
        recv.StartListening();
    }

    public static void ReceivePacket(object sender, ArtNetPacket args)
    {
        lock (routers)
        {
            if (args is ArtDmxPacket packet)
            {
                foreach (var router in routers.Values)
                {
                    router.Route(packet);
                }
            }
        }
    }

    public static bool TryGetRouter(this World w, out ArtNetRouter router)
    {
        return routers.TryGetValue(w, out router);
    }
    
    public static ArtNetRouter SetupOrGetRouter(this World w)
    {
        lock (routers)
        {
            if (w.TryGetRouter(out ArtNetRouter router))
                return router;

            router = new();
            routers.Add(w, router);
            Stagefright.Msg($"Instantiated router for world {w.Name}");
            return router;
        }
    }

    public static bool TryDestroyRouter(this World w)
    {
        lock (routers)
        {
            bool flag = w.TryGetRouter(out var router);
            router?.DestroyUniverses();
            if (flag)
                routers.Remove(w);
            
            return flag;
        }
    }
}
