using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ModUtilsTest;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("baer1.LethalModUtils")]
public class ModUtilsTest : BaseUnityPlugin
{
    public static ModUtilsTest Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    public AudioManager AudioManager { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        AudioManager = new AudioManager();
        AudioManager.LoadAudioFiles();

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        return;

        void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
            Logger.LogDebug("Patching...");
            Harmony.PatchAll();
            Logger.LogDebug("Finished patching!");
        }
    }
}
