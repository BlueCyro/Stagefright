using FrooxEngine;
using Elements.Core;
using System.Net.Sockets;
using FrooxEngine.ProtoFlux;

namespace Stagefright;

public static class ArtNetBridge
{
    public const string LISTEN_ADDRESS = "127.0.0.1";
    public const int ARTNET_PORT = 6454;
    public const string UNIVERSE_PREFIX = "DMXUniverse:";
    public static UdpClient client = new(ARTNET_PORT);
    public static BiDictionary<World, ArtNetRouter> routers = new();
    private static CancellationTokenSource tknSrc = new();

    public static bool TryGetRouter(this World w, out ArtNetRouter router)
    {
        return routers.TryGetSecond(w, out router);
    }
    
    public static ArtNetRouter SetupRouter(this World w, bool replaceExisting = false)
    {
        if (w.TryGetRouter(out ArtNetRouter router))
            return router;

        router = new();
        routers.Add(w, router);
        return router;
    }

    public static void StartListening()
    {
        Stagefright.Msg($"Starting client connection on: {LISTEN_ADDRESS}:{ARTNET_PORT}");
        client.Connect(LISTEN_ADDRESS, ARTNET_PORT);
        Stagefright.Msg("Starting ArtNet listener loop");
        Task.Run(async () => await ConnectionLoop(tknSrc.Token));
    }

    public static void StopListening()
    {
        tknSrc.Cancel();
    }

    public static async Task ConnectionLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var data = await client.ReceiveAsync();
            foreach(var router in routers)
            {
                router.Second.Route(new(data.Buffer));
            }
        }
    }

    public static void SetupUniverse(this Slot root, UniverseInfo info)
    {
        World w = root.World;
        Stagefright.Msg($"Building Universe");
        if (!w.TryGetRouter(out ArtNetRouter router))
        {
            Stagefright.Msg($"No router exists for {w.Name}, adding router to world dictionary");
            router = w.SetupRouter();
        }

        Stagefright.Msg($"Setting up universe {info.Universe} in world {w.Name} with size {info.Count}");
        router.SetupUniverse(info, out List<ValueStream<float>> streams);

        var space = root.FindSpace("DMXUniverse") ?? root.AttachComponent<DynamicVariableSpace>();
        space.SpaceName.Value ??= "DMXUniverse";
        
        Slot variableRoot = root.FindChildOrAdd("<b><i>variable<color=hero.purple>(</color>s<color=hero.purple>)</color></i></b>", false);
        Slot streamSlot = variableRoot.FindChildOrAdd("DMX Stream Variables", false);
        streamSlot.OrderOffset = 1024;

        space.ClearAllValuesOfType<IValue<float>>();
        for (int i = 0; i < streams.Count; i++)
            streamSlot.CreateVariable<IValue<float>>((i + 1).ToString(), streams[i], false); // DMX channels are 1-indexed. Ouch.

        ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulse(root, "DMXDevice.refresh", false);
        return;
    }

    public static void SetupUniverse(this Slot root)
    {
        if (TryParseUniverseInfo(root.Name, out UniverseInfo parsed))
        {
            SetupUniverse(root, parsed);
        }
    }

    public static bool TryParseUniverseInfo(this string str, out UniverseInfo data)
    {
        if (!string.IsNullOrEmpty(str) && str.StartsWith(UNIVERSE_PREFIX))
        {
            str = str.Substring(UNIVERSE_PREFIX.Length);
            int divider = str.IndexOf(':');

            bool uniParsed = int.TryParse(str.Substring(0, divider), out int universe);
            bool countParsed = int.TryParse(str.Substring(divider + 1), out int streamCount);
            bool fullyParsed = uniParsed && countParsed;

            data = fullyParsed ? new(universe, streamCount) : new(-1, -1);
            return fullyParsed;
        }
        else
        {
            data = new(-1, -1);
            return false;
        }
    }

}
