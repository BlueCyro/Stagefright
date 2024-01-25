using FrooxEngine;
using FrooxEngine.Headless;
using HarmonyLib;
using System.Reflection;

namespace Stagefright;
public static class HeadlessHelper
{

    /// <summary>
    /// True if the mod is running on a headless client
    /// </summary>
    public static bool IsHeadless
    {
        get
        {
            return _isHeadless ??= AppDomain.CurrentDomain.GetAssemblies().Any(a => // Overkill, but better safe than sorry. It only happens once anyways.
            {
                IEnumerable<Type> types;
                try
                {
                    types = a.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }
                return types.Any(t => t != null && t.Namespace == "FrooxEngine.Headless");
            });
        }
    }

    private static bool? _isHeadless;



    internal static void PatchHeadless(Harmony harmony)
    {
        var target = typeof(HeadlessCommands).GetMethod(nameof(HeadlessCommands.SetupCommonCommands));
        var patch = typeof(HeadlessHelper).GetMethod(nameof(StagefrightCommands));
        harmony.Patch(target, postfix: new(patch));
    }



    public static void StagefrightCommands(CommandHandler handler)
    {
        GenericCommand command =
        new
        (
            "setupDMX",
            "Sets up all DMX universes that can be found in the currently focused world",
            "",
            SetupDMX
        );
        handler.RegisterCommand(command);
    }



    private static void SetupDMX(CommandHandler handler, World world, List<string> args)
    {
        world.RunSynchronously(() => {
            world.AllSlots.ToList().ForEach(s => s.TrySetupStage());
        });
    }
}