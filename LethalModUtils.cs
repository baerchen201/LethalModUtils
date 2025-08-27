using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace LethalModUtils;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalModUtils : BaseUnityPlugin
{
    public static LethalModUtils Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private ConfigEntry<bool> preloadAudio = null!;
    public bool PreloadAudio { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        InitConfig();
        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        return;

        void InitConfig()
        {
            const string AUDIO = "Audio";
            preloadAudio = Config.Bind(
                AUDIO,
                "PreloadAudio",
                true,
                "Whether to pre-load audio into RAM"
            );
            PreloadAudio = preloadAudio.Value;
        }

        void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
            Logger.LogDebug("Patching...");
            Harmony.PatchAll();
            Logger.LogDebug("Finished patching!");
        }
    }
}
