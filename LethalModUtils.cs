using System;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

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
        Logger = base.Logger;
        Instance = this;

        InitConfig();
        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        return;

        void InitConfig()
        {
            const string GENERAL = "General";
            exportStaticData = Config.Bind(
                GENERAL,
                nameof(ExportStaticData),
                false,
                "Set to true to export static game data to file on next opportunity"
            );
            exportStaticData.SettingChanged += (_, _) =>
            {
                if (exportStaticData.Value && StartOfRound.Instance)
                    ExportStaticData(StartOfRound.Instance);
            };

            const string AUDIO = "Audio";
            preloadAudio = Config.Bind(
                AUDIO,
                nameof(PreloadAudio),
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

    public void ExportStaticData(StartOfRound __instance)
    {
        Logger.LogInfo("Requested static data export...");
        exportStaticData.Value = false;

        try
        {
            using var f = File.Open(
                Path.Combine(Environment.CurrentDirectory, $"{nameof(StaticData)}.json"),
                FileMode.Create,
                FileAccess.Write
            );
            using var writer = new StreamWriter(f, Encoding.UTF8);
            using var jsonWriter = new JsonTextWriter(writer);
            StaticData
                .ImportUtil.Import(
                    GameNetworkManager.Instance?.gameVersionNum ?? -1,
                    __instance.allItemsList,
                    __instance.levels
                )
                .Serialize(jsonWriter);
            jsonWriter.Flush();
            writer.Flush();
            f.Flush();
            Logger.LogInfo(
                $"Exported static data to {Path.GetFullPath(f.Name)} ({f.Position} bytes)"
            );
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
