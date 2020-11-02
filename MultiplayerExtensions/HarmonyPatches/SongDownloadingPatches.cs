using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp;
using HarmonyLib;
using HMUI;
using IPA.Logging;
using IPA.Utilities;
using MultiplayerExtensions.OverrideClasses;
using MultiplayerExtensions.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace MultiplayerExtensions.HarmonyPatches
{

    [HarmonyPatch(typeof(LobbyGameStateController), nameof(LobbyGameStateController.HandleMenuRpcManagerStartedLevel),
        new Type[] { // List the Types of the method's parameters.
        typeof(string), typeof(BeatmapIdentifierNetSerializable), typeof(GameplayModifiers), typeof(float) })]
    public class LobbyGameStateController_HandleMenuRpcManagerStartedLevel
    {
        public static LobbyGameStateController? LobbyGameStateController;
        public static string? LastUserId;

        static bool Prefix(ref string userId, ref BeatmapIdentifierNetSerializable beatmapId, ref GameplayModifiers gameplayModifiers, ref float startTime, LobbyGameStateController __instance)
        {
            Plugin.Log?.Debug($"LobbyGameStateController.HandleMenuRpcManagerStartedLevel");

            if (SongCore.Loader.GetLevelById(beatmapId.levelID) != null)
                Plugin.Log?.Debug($"Level is loaded.");
            LobbyGameStateController = __instance;
            LastUserId = userId;
            return true;
        }
    }

    [HarmonyPatch(typeof(MultiplayerLevelLoader), nameof(MultiplayerLevelLoader.LoadLevel),
    new Type[] { // List the Types of the method's parameters.
        typeof(BeatmapIdentifierNetSerializable), typeof(GameplayModifiers), typeof(float) })]
    public class MultiplayerLevelLoader_LoadLevel
    {
        public static MultiplayerLevelLoader? MultiplayerLevelLoader;
        public static readonly string CustomLevelPrefix = "custom_level_";
        private static string? LoadingLevelId;

        static bool Prefix(ref BeatmapIdentifierNetSerializable beatmapId, ref GameplayModifiers gameplayModifiers, ref float initialStartTime, MultiplayerLevelLoader __instance)
        {
            string? levelId = beatmapId.levelID;
            if (SongCore.Loader.GetLevelById(levelId) != null)
                return true;
            string? hash = Utilities.Utilities.LevelIdToHash(beatmapId.levelID);
            if (hash == null)
            {
                Plugin.Log?.Info($"Could not get a hash from beatmap with LevelId {beatmapId.levelID}");
                return true;
            }

            MultiplayerLevelLoader = __instance;
            BeatmapIdentifierNetSerializable bmId = beatmapId;
            GameplayModifiers modifiers = gameplayModifiers;
            float startTime = initialStartTime;

            if (LoadingLevelId == null || LoadingLevelId != levelId)
            {
                LoadingLevelId = levelId;
                var downloadTask = Downloader.TryDownloadSong(levelId, CancellationToken.None, success =>
                {
                    try
                    {
                        if (success)
                        {
                            //Plugin.Log?.Debug($"Triggering 'LobbyGameStateController.HandleMenuRpcManagerStartedLevel' after level download.");
                            //LobbyGameStateController_HandleMenuRpcManagerStartedLevel.LobbyGameStateController.HandleMenuRpcManagerStartedLevel(LobbyGameStateController_HandleMenuRpcManagerStartedLevel.LastUserId, bmId, modifiers, startTime);
                        }
                        else
                            Plugin.Log?.Warn($"TryDownloadSong was unsuccessful.");
                        MultiplayerLevelLoader.LoadLevel(bmId, modifiers, startTime);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log?.Warn($"Error in TryDownloadSong callback: {ex.Message}");
                        Plugin.Log?.Debug(ex);
                    }
                    finally
                    {
                        LoadingLevelId = null;
                    }
                });
                return false;
            }
            // LoadingLevelId = null;
            return true;
        }

    }

    [HarmonyPatch(typeof(LobbyPlayersDataModel), "HandleMenuRpcManagerSelectedBeatmap", MethodType.Normal)]
    class SetPlayerLevelPatch
    {
        static bool Prefix(string userId, BeatmapIdentifierNetSerializable beatmapId, LobbyPlayersDataModel __instance)
        {
            if (beatmapId != null)
            {
                var hash = Utilities.Utilities.LevelIdToHash(beatmapId.levelID);
                if (hash != null)
                {
                    Plugin.Log?.Debug($"'{userId}' selected song '{hash}'.");
                    if (SongCore.Loader.GetLevelById(hash) != null)
                    {
                        Plugin.Log?.Debug($"Custom song '{hash}' loaded.");
                        return true;
                    }

                    Plugin.Log?.Debug("Getting song characteristics");
                    var characteristicCollection = __instance.GetField<BeatmapCharacteristicCollectionSO, LobbyPlayersDataModel>("_beatmapCharacteristicCollection");
                    var characteristic = characteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);

                    Plugin.Log?.Debug("Setting song preview");
                    var beatmap = BeatSaver.Client.Hash(Utilities.Utilities.LevelIdToHash(beatmapId.levelID));
                    beatmap.ContinueWith(r =>
                    {
                        HMMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            __instance.SetPlayerBeatmapLevel(userId, new PreviewBeatmapLevelStub(beatmapId.levelID, r.Result), beatmapId.difficulty, characteristic);
                        });
                    });

                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LobbyPlayersDataModel), "SetLocalPlayerBeatmapLevel", MethodType.Normal)]
    class SetLocalPlayerLevelPatch
    {
        static bool Prefix(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic, LobbyPlayersDataModel __instance)
        {
            if (Utilities.Utilities.LevelIdToHash(levelId) != null)
            {
                Plugin.Log?.Debug($"Local user selected song '{levelId}'.");
                if (SongCore.Loader.GetLevelById(levelId) != null)
                {
                    Plugin.Log?.Debug($"Custom song '{levelId}' loaded.");
                    return true;
                }

                Plugin.Log?.Debug("Updating RPC");
                var menuRpcManager = __instance.GetField<IMenuRpcManager, LobbyPlayersDataModel>("_menuRpcManager");
                menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));

                Plugin.Log?.Debug("Setting song preview");
                var beatmap = BeatSaver.Client.Hash(Utilities.Utilities.LevelIdToHash(levelId));
                beatmap.ContinueWith(r =>
                {
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        __instance.SetPlayerBeatmapLevel(__instance.localUserId, new PreviewBeatmapLevelStub(levelId, r.Result), beatmapDifficulty, characteristic);
                    });
                });

                return false;
            }
            return true;
        }
    }
}

public class UIHelper : MonoBehaviour // This is needed because a static class cannot inherit from Monobehaviour
{
    public static void RefreshUI()
    {
        Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().FirstOrDefault().UpdateCustomSongs();
    }
}