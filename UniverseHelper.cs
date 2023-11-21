using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace Stagefright;

public static class UniverseHelper
{
    public const string UNIVERSE_PREFIX = "DMXUniverse:";
    public static void BuildSet(Slot s)
    {
        var field = s.AttachComponent<ReferenceField<Slot>>();
        field.Reference.Changed += c =>
        {
            field.Reference.Target?.SetupUniverse();
        };
        s.OpenInspectorForTarget();
    }

    public static void SetupWorldUniverses(Slot temp)
    {
        lock (ArtNetBridge.routers)
        {
            var world = temp.World;
            var all = world.AllSlots.ToList();

            ArtNetRouter router = world.SetupOrGetRouter();
            router.DestroyUniverses();
            all.ForEach(s => s.SetupUniverse());

            temp.Destroy();
        }
    }
    
    public static void SetupUniverse(this Slot root)
    {
        if (TryParseUniverseInfo(root.Name, out UniverseInfo parsed))
        {
            SetupUniverse(root, parsed);
        }
    }

    public static void SetupUniverse(this Slot root, UniverseInfo info)
    {
        lock (ArtNetBridge.routers)
        {
            World world = root.World;
            ArtNetRouter router = world.SetupOrGetRouter();

            Stagefright.Msg($"Setting up universe {info.Universe} in world {world.Name} with size {info.Count}");

            if (router.Contains(info.Universe))
                router.DestroyUniverse(info.Universe);
            
            var streams = router.SetupUniverse(info, world);

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
