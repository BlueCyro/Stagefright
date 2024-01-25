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

        if (HeadlessHelper.IsHeadless)
        {
            Msg("Headless detected! Running headless-specific setup...");
            HeadlessHelper.PatchHeadless(harmony);
        }
        else
        {
            Engine.Current.RunPostInit(() =>
            {
                ArtNetBridge.StartListening();
                DevCreateNewForm.AddAction("ArtNet", "Set up individual universes", StageHelper.BuildStage);
                DevCreateNewForm.AddAction("ArtNet", "Set up all universes", StageHelper.SetupWorldStages);
            });
        }
    }
}
