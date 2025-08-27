using System;
using System.IO;
using GameNetcodeStuff;
using HarmonyLib;
using LethalModUtils;
using UnityEngine;

namespace ModUtilsTest;

public class AudioManager
{
    public AudioClip? EnterBuildingSound { get; private set; }
    private const string enterBuildingSound = "enter.wav";

    public AudioClip? JumpSound { get; private set; }
    private const string jumpSound = "jump.wav";

    public AudioClip? ShipBackgroundMusic { get; private set; }
    private const string shipBackgroundMusic = "ship.wav";

    public AudioClip? PlanetBackgroundMusic { get; private set; }
    private const string planetBackgroundMusic = "planet.wav";

    private Uri GetUri(string fileName) =>
        new(
            Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location) ?? string.Empty,
                "Sounds",
                fileName
            )
        );

    public void LoadAudioFiles()
    {
        EnterBuildingSound = Audio.TryLoad(GetUri(enterBuildingSound), TimeSpan.FromSeconds(10));
        JumpSound = Audio.TryLoad(GetUri(jumpSound), TimeSpan.FromSeconds(10));

        ShipBackgroundMusic = Audio.TryLoad(GetUri(shipBackgroundMusic), TimeSpan.FromSeconds(10));
        PlanetBackgroundMusic = Audio.TryLoad(
            GetUri(planetBackgroundMusic),
            TimeSpan.FromSeconds(10)
        );
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.PlayJumpAudio))]
    internal static class PlayerControllerB_PlayJumpAudio
    {
        private static bool Prefix(ref PlayerControllerB __instance)
        {
            var sound = ModUtilsTest.Instance.AudioManager.JumpSound;
            if (sound == null)
                return true;
            __instance.movementAudio.PlayOneShot(sound);
            return false;
        }
    }

    [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.PlayAudioAtTeleportPositions))]
    internal static class EntranceTeleport_PlayAudioAtTeleportPositions
    {
        private static bool Prefix(ref EntranceTeleport __instance)
        {
            var sound = ModUtilsTest.Instance.AudioManager.EnterBuildingSound;
            if (sound == null)
                return true;
            __instance.entrancePointAudio.PlayOneShot(sound);
            __instance.exitPointAudio.PlayOneShot(sound);
            return false;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
    internal static class StartOfRound_Awake
    {
        private static void a(Audio.AudioPlayer player)
        {
            try
            {
                if (
                    StartOfRound.Instance.inShipPhase
                    || GameNetworkManager.Instance.localPlayerController.isPlayerDead
                )
                {
                    Fade(player, false);
                }
                else if (player.State == Audio.AudioPlayer.PlayerState.Paused)
                {
                    player.Resume();
                    player.Volume = 0f;
                }
                else
                {
                    Fade(player, true);
                    player.BypassEffects = GameNetworkManager
                        .Instance
                        .localPlayerController
                        .isInsideFactory;
                }
            }
            catch (NullReferenceException e)
            {
                ModUtilsTest.Logger.LogWarning(e);
            }
        }

        private static void b(Audio.AudioPlayer player)
        {
            try
            {
                if (
                    !StartOfRound.Instance.inShipPhase
                    && !GameNetworkManager.Instance.localPlayerController.isPlayerDead
                )
                {
                    Fade(player, false);
                }
                else if (player.State == Audio.AudioPlayer.PlayerState.Paused)
                {
                    player.Resume();
                    player.Volume = 0f;
                }
                else
                {
                    Fade(player, true);
                }
            }
            catch (NullReferenceException e)
            {
                ModUtilsTest.Logger.LogWarning(e);
            }
        }

        private static void Fade(Audio.AudioPlayer player, bool value)
        {
            if (value)
                player.Volume = player.Volume < 0.48f ? Mathf.Lerp(player.Volume, 5f, 0.02f) : 5f;
            else
            {
                if (player.Volume <= 0.02f)
                    player.Stop();
                else
                    player.Volume = Mathf.Lerp(player.Volume, 0f, 0.02f);
            }
        }

        private static void Postfix()
        {
            var shipSound = ModUtilsTest.Instance.AudioManager.ShipBackgroundMusic;
            var planetSound = ModUtilsTest.Instance.AudioManager.PlanetBackgroundMusic;
            if (shipSound != null)
                shipSound.Play(b);
            if (planetSound != null)
                planetSound
                    .CreatePlayer(a)
                    ._audioSource.gameObject.AddComponent<AudioLowPassFilter>()
                    .cutoffFrequency = 200f;
        }
    }
}
