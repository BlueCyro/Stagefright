using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using System.Text;

namespace Stagefright;

public class Stagefright : ResoniteMod
{
    public override string Name => "StageFright";
    public override string Author => "Cyro";
    public override string Version => "0.1.0";
    public override string Link => "https://github.com/RileyGuy/Stagefright";
    public static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Harmony harmony = new("net.Cyro.Stagefright");
        Config = GetConfiguration();
        Config?.Save(true);

        Engine.Current.RunPostInit(() =>
        {
            ArtNetBridge.StartListening();
            DevCreateNewForm.AddAction("ArtNet", "Set up individual universes", UniverseHelper.BuildSet);
            DevCreateNewForm.AddAction("ArtNet", "Set up all universes", UniverseHelper.SetupWorldUniverses);
            

            Engine.Current.WorldManager.WorldRemoved += w =>
            {
                var worlds = Engine.Current.WorldManager.Worlds;
                if (w.TryGetRouter(out ArtNetRouter router))
                {
                    StringBuilder sb = new($"Removing ArtNet router for world: {w.Name}\nRouter contained universes: ");
                    var indicies = router.GetUniverseIndicies();
                    int count = indicies.Count();
                    int index = 0;
                    foreach (int uni in indicies)
                    {
                        sb.Append($"{uni}{(index++ < count - 1 ? index == count - 1 ? " and " : ", " : "")}"); // The ternary ever
                    }

                    w.TryDestroyRouter();
                    Stagefright.Msg(sb.ToString());
                }
            };
        });
    }
}
