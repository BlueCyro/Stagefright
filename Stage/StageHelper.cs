using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace Stagefright;

public static class StageHelper
{
    public const string STAGE_IDENTIFIER = "DMXUniverse";
    public const string STAGE_PREFIX = STAGE_IDENTIFIER + ":";
    public const string VARIABLE_SLOT_TAG = STAGE_IDENTIFIER + ".variable_slot";
    public const string DMX_DEVICE_REFRESH_TAG = "DMXDevice.refresh";



    public static void BuildStage(Slot s)
    {
        var field = s.AttachComponent<ReferenceField<Slot>>();
        var comment = s.AttachComponent<Comment>();
        comment.Text.Value = "Place your stage slot into the reference field!";
        
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
        root.DestroyStage();
        world.DestroyUniverse(info.Universe);
        root.RunInUpdates(3, () =>
        {

            Stagefright.Msg($"Setting up universe {info.Universe} in world {world.Name} with size {info.Count}");

            // Set up a new universe for this world
            Universe uni = world.SetupUniverse(info.Count, info.Universe);

            // Find the variable space root
            var space = root.FindSpace(STAGE_IDENTIFIER) ?? root.AttachComponent<DynamicVariableSpace>();
            space.SpaceName.Value ??= STAGE_IDENTIFIER;
            
            // Create a variables slot
            Slot variableRoot = root.FindChild(s => s.Tag == VARIABLE_SLOT_TAG);

            if (variableRoot == null)
            {
                Slot varRoot = root.AddSlot("<b><i>variable<color=hero.purple>(</color>s<color=hero.purple>)</color></i></b>", false);
                varRoot.OrderOffset = 1024;
                Slot vars = varRoot.AddSlot("DMX Stream Variables");
                vars.Tag = VARIABLE_SLOT_TAG;
                variableRoot = vars;
            }

            for (int i = 0; i < uni.ChannelCount; i++)
            {
                // Create a new variable in their place
                string variableName = (i + 1).ToString();
                if (i < uni.Streams.Length)
                    variableRoot.CreateVariable<IValue<float>>(variableName, uni.Streams[i], false); // DMX channels are 1-indexed. Ouch.
            }


            ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulse(root, DMX_DEVICE_REFRESH_TAG, false);

        });
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



    public static void DestroyStage(this Slot slot)
    {
        if (slot.Name.TryParseUniverseInfo(out StageInfo data))
        {
            Stagefright.Msg($"Destroying stage {data.Universe} with {data.Count} channels in {slot.World.Name}");
            Slot variableRoot = slot.FindChild(s => s.Tag == VARIABLE_SLOT_TAG);
            
            if (variableRoot != null)
            {
                var streamVars = variableRoot.GetComponents<DynamicReferenceVariable<IValue<float>>>();
                for (int i = 0; i < 512; i++)
                {
                    streamVars.FirstOrDefault(c => c.VariableName.Value == (i + 1).ToString())?.Destroy();
                }
            }
            slot.World.DestroyUniverse(data.Universe);
        }
    }



    public static void DestroyWorldStages(this World world)
    {
        Stagefright.Msg($"Destroying all stages in {world.Name}!");
        world.AllSlots.ToList().ForEach(s => s.DestroyStage());
        world.DestroyUniverses();
    }



    public static void DestroyWorldStages(Slot temp)
    {
        temp.World.DestroyWorldStages();
        temp.Destroy();
    }
}
