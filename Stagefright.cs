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

        ArtNetBridge.StartListening();

        if (HeadlessHelper.IsHeadless)
        {
            Msg("Headless detected! Running headless-specific setup...");
            HeadlessHelper.PatchHeadless(harmony);
        }
        else
        {
            Engine.Current.RunPostInit(() =>
            {
                DevCreateNewForm.AddAction("Stagefright", "Set up individual stages", StageHelper.BuildStage);
                DevCreateNewForm.AddAction("Stagefright", "Set up all stages", StageHelper.SetupWorldStages);
                DevCreateNewForm.AddAction("Stagefright", "Destroy all stages", StageHelper.DestroyWorldStages);
            });
        }
    }
}
