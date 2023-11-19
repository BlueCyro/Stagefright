using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using System.Text;

namespace Stagefright;

public class Stagefright : ResoniteMod
{
    public override string Name => "StageFright";
    public override string Author => "Cyro";
    public override string Version => "1.0.0";
    public override string Link => "resonite.com";
    public static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Harmony harmony = new("net.Cyro.Stagefright");
        Config = GetConfiguration();
        Config?.Save(true);

        Engine.Current.RunPostInit(() =>
        {
            ArtNetBridge.StartListening();
            DevCreateNewForm.AddAction("ArtNet", "Set up individual universes", BuildSet);
            DevCreateNewForm.AddAction("ArtNet", "Set up all universes", SetupWorldUniverses);
            

            Engine.Current.WorldManager.WorldRemoved += w =>
            {
                var worlds = Engine.Current.WorldManager.Worlds;
                if (ArtNetBridge.routers.TryGetSecond(w, out ArtNetRouter router))
                {
                    StringBuilder sb = new($"Removing ArtNet router for world: {w.Name}\nRouter contained universes: ");
                    var indicies = router.GetUniverseIndicies();
                    int count = indicies.Count();
                    var enumerator = indicies.GetEnumerator();

                    for (int i = 0; i < count; i++)
                    {

                        sb.Append((i == count - 2 ? ", " : " and ") + enumerator.Current);
                        enumerator.MoveNext();
                    }

                    router.DestroyUniverses();
                    ArtNetBridge.routers.RemoveByFirst(w);
                    Stagefright.Msg(sb.ToString());
                }
            };
        });
    }

    public static void BuildSet(Slot s)
    {
        var field = s.AttachComponent<ReferenceField<Slot>>();
        field.Reference.Changed += c =>
        {
            if (field.Reference.Target != null)
            {
                ArtNetBridge.SetupUniverse(field.Reference.Target);
            }
        };
        s.OpenInspectorForTarget();
    }

    public static void SetupWorldUniverses(Slot temp)
    {
        var all = temp.World.AllSlots;

        var universes = all.Select(s => new { isUniverse = s.Name.TryParseUniverseInfo(out UniverseInfo uniInfo), info = uniInfo, slot = s })
                            .Where(s => s.isUniverse)
                            .GroupBy(s => s.info.Universe)
                            .Select(s => s.First())
                            .ToList();

        ArtNetRouter router = temp.World.SetupRouter();
        
        router.DestroyUniverses();

        foreach (var s in universes)
        {
            if (!router.ContainsUniverse(s.info.Universe))
                s.slot.SetupUniverse(s.info);
        }

        temp.Destroy();
    }
}
