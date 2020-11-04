using BeatSaverSharp;
using HarmonyLib;
using IPA.Utilities;
using MultiplayerExtensions.Downloaders;
using MultiplayerExtensions.OverrideClasses;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if DEBUG
using HMUI;
#endif

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace MultiplayerExtensions.HarmonyPatches
{

    [HarmonyPatch(typeof(LobbyGameStateController), nameof(LobbyGameStateController.HandleMenuRpcManagerStartedLevel),
        new Type[] { typeof(string), typeof(BeatmapIdentifierNetSerializable), typeof(GameplayModifiers), typeof(float) })]
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
    new Type[] { typeof(BeatmapIdentifierNetSerializable), typeof(GameplayModifiers), typeof(float) })]
    public class MultiplayerLevelLoader_LoadLevel
    {
        public static MultiplayerLevelLoader? MultiplayerLevelLoader;
        public static readonly string CustomLevelPrefix = "custom_level_";
        private static string? LoadingLevelId;

        static bool Prefix(ref BeatmapIdentifierNetSerializable beatmapId, ref GameplayModifiers gameplayModifiers, ref float initialStartTime, MultiplayerLevelLoader __instance)
        {
            string? levelId = beatmapId.levelID;
            if (SongCore.Loader.GetLevelById(levelId) != null)
            {
                Plugin.Log?.Debug($"Level with ID '{levelId}' already exists.");
                return true;
            }
            string? hash = Utilities.Util.LevelIdToHash(beatmapId.levelID);
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

                Plugin.Log?.Debug($"Attempting to download level with ID '{levelId}'...");
                Task? downloadTask = SongDownloader.TryDownloadSong(levelId, CancellationToken.None, success =>
                {
                    try
                    {
                        if (success)
                        {
                            Plugin.Log?.Debug($"Level with ID '{levelId}' was downloaded successfully.");
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
                string? hash = Utilities.Util.LevelIdToHash(beatmapId.levelID);
                if (hash != null)
                {
                    Plugin.Log?.Debug($"'{userId}' selected song '{hash}'.");
                    if (SongCore.Loader.GetLevelById(hash) != null)
                    {
                        Plugin.Log?.Debug($"Custom song '{hash}' loaded.");
                        return true;
                    }

                    Plugin.Log?.Debug("Getting song characteristics");
                    BeatmapCharacteristicCollectionSO? characteristicCollection = __instance.GetField<BeatmapCharacteristicCollectionSO, LobbyPlayersDataModel>("_beatmapCharacteristicCollection");
                    BeatmapCharacteristicSO? characteristic = characteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);

                    Plugin.Log?.Debug("Setting song preview");
                    Task<Beatmap>? beatmap = BeatSaver.Client.Hash(Utilities.Util.LevelIdToHash(beatmapId.levelID));
                    beatmap.ContinueWith(r =>
                    {
                        if (r.IsCanceled)
                        {
                            Plugin.Log?.Debug($"Metadata retrieval for {beatmapId.levelID} was canceled.");
                            return;
                        }
                        else if (r.IsFaulted)
                        {
                            Plugin.Log?.Error($"Error retrieving metadata for {beatmapId.levelID}: {r.Exception.Message}");
                            Plugin.Log?.Debug(r.Exception);
                        }
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
            string? hash = Utilities.Util.LevelIdToHash(levelId);
            if (hash != null)
            {
                Plugin.Log?.Debug($"Local user selected song '{levelId}'.");
                if (SongCore.Loader.GetLevelById(levelId) != null)
                {
                    Plugin.Log?.Debug($"Custom song '{levelId}' loaded.");
                    return true;
                }

                Plugin.Log?.Debug("Updating RPC");
                IMenuRpcManager? menuRpcManager = __instance.GetField<IMenuRpcManager, LobbyPlayersDataModel>("_menuRpcManager");
                menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));

                Plugin.Log?.Debug("Setting song preview");
                Task<Beatmap>? beatmap = BeatSaver.Client.Hash(hash);
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