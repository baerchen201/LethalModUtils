using System;
using System.IO;
using HarmonyLib;
using LethalModUtils;
using UnityEngine;

namespace ModUtilsTest;

public static class LogoReplacement
{
    public static Sprite? LogoImage { get; private set; }
    private const string logoImage = "logo.png";

    private static Uri GetUri(string fileName) =>
        new(
            Path.Combine(
                Path.GetDirectoryName(typeof(LogoReplacement).Assembly.Location) ?? string.Empty,
                "Images",
                fileName
            )
        );

    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Start))]
    internal static class MenuManager_Start
    {
        private static void Postfix(ref MenuManager __instance)
        {
            if (LogoImage == null)
                return;

            var mainLogo = __instance.transform.parent.Find(
                "MenuContainer/MainButtons/HeaderImage"
            );
            if (mainLogo != null)
                mainLogo.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = LogoImage;

            var loadingScreen = __instance.transform.parent.Find(
                "MenuContainer/LoadingScreen/Image"
            );
            if (loadingScreen != null)
                loadingScreen.GetComponent<UnityEngine.UI.Image>().sprite = LogoImage;
        }
    }

    public static async void LoadImage()
    {
        LogoImage = (
            await Image.TryLoadAsync(GetUri(logoImage), TimeSpan.FromSeconds(10))
        )?.ToSprite();
    }
}
