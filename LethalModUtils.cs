using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace LethalModUtils;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalModUtils : BaseUnityPlugin
{
    internal ConfigEntry<bool> exportStaticData = null!;
    private ConfigEntry<bool> preloadAudio = null!;
    public static LethalModUtils Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    public bool PreloadAudio { get; private set; }

    private void Awake()
    {
        const string SECTION_AUDIO = "Audio";
        Logger = base.Logger;
        Instance = this;

        preloadAudio = Config.Bind(
            SECTION_AUDIO,
            nameof(PreloadAudio),
            true,
            "Whether to pre-load audio into RAM"
        );
        PreloadAudio = preloadAudio.Value;

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
