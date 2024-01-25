using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace Stagefright;

public static class StageHelper
{
    public const string STAGE_PREFIX = "DMXUniverse:";



    public static void BuildStage(Slot s)
    {
        var field = s.AttachComponent<ReferenceField<Slot>>();
        field.Reference.Changed += c =>
        {
            field.Reference.Target?.TrySetupStage();
        };
        s.OpenInspectorForTarget();
    }



    public static void SetupWorldStages(Slot temp)
    {
        var world = temp.World;
        var all = world.AllSlots.ToList();

        world.DestroyUniverses();
        all.ForEach(s => s.TrySetupStage());

        temp.Destroy();
    }
    


    public static bool TrySetupStage(this Slot root)
    {
        if (TryParseUniverseInfo(root.Name, out StageInfo parsed))
        {
            root.SetupStage(parsed);
            return true;
        }

        return false;
    }



    public static void SetupStage(this Slot root, StageInfo info)
    {
        // Pre-emptively destroy an existing universe if it already exists
        World world = root.World;
        world.DestroyUniverse(info.Universe);


        Stagefright.Msg($"Setting up universe {info.Universe} in world {world.Name} with size {info.Count}");

        // Set up a new universe for this world
        Universe uni = world.SetupUniverse(info.Count, info.Universe);

        // Find the variable space root
        var space = root.FindSpace("DMXUniverse") ?? root.AttachComponent<DynamicVariableSpace>();
        space.SpaceName.Value ??= "DMXUniverse";
        
        // Create a variables slot
        Slot variableRoot = root.FindChildOrAdd("<b><i>variable<color=hero.purple>(</color>s<color=hero.purple>)</color></i></b>", false);
        Slot streamSlot = variableRoot.FindChildOrAdd("DMX Stream Variables", false);
        streamSlot.OrderOffset = 1024;


        // Blow up all old DMX variables in the space
        for (int i = 0; i < 512; i++)
        {
            string variableName = (i + 1).ToString();
            space.GetManager<IField<float>>(variableName, false)?.DeleteAllReadable();

            // Create a new variable in their place if we're within the new universe's stream count
            if (i < uni.Streams.Length)
                streamSlot.CreateVariable<IValue<float>>((i + 1).ToString(), uni.Streams[i], false); // DMX channels are 1-indexed. Ouch.
        }


        ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulse(root, "DMXDevice.refresh", false);
    }



    public static bool TryParseUniverseInfo(this string str, out StageInfo data)
    {
        if (!string.IsNullOrEmpty(str) && str.StartsWith(STAGE_PREFIX))
        {
            str = str.Substring(STAGE_PREFIX.Length);
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
