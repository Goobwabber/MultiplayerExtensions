using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using HMUI;
using IPA.Logging;
using IPA.Utilities;
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
        public static LobbyGameStateController LobbyGameStateController;
        public static string LastUserId;

        static bool Prefix(ref string userId, ref BeatmapIdentifierNetSerializable beatmapId, ref GameplayModifiers gameplayModifiers, ref float startTime, LobbyGameStateController __instance)
        {
            Plugin.Log?.Debug($"LobbyGameStateController.HandleMenuRpcManagerStartedLevel");
            if (SongCore.Loader.GetLevelById(beatmapId.levelID) != null)
                Plugin.Log.Debug($"Level is loaded.");
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
        public static MultiplayerLevelLoader MultiplayerLevelLoader;
        public static readonly string CustomLevelPrefix = "custom_level_";
        private static string LoadingLevelId;

        static bool Prefix(ref BeatmapIdentifierNetSerializable beatmapId, ref GameplayModifiers gameplayModifiers, ref float initialStartTime, MultiplayerLevelLoader __instance)
        {
            // Loading here doesn't work
            return true;
            MultiplayerLevelLoader = __instance;
            string levelId = beatmapId?.levelID;
            BeatmapIdentifierNetSerializable bmId = beatmapId;
            GameplayModifiers modifiers = gameplayModifiers;
            float startTime = initialStartTime;
            if (levelId != null && levelId.StartsWith(CustomLevelPrefix))
            {
                if (SongCore.Loader.GetLevelById(levelId) != null)
                {
                    LoadingLevelId = null;
                    return true;
                }
                if (LoadingLevelId == null || LoadingLevelId != levelId)
                {
                    LoadingLevelId = levelId;
                    var downloadTask = Downloader.TryDownloadSong(levelId, CancellationToken.None, r =>
                    {
                        if (r)
                        {
                            Plugin.Log?.Debug($"Triggering 'LobbyGameStateController.HandleMenuRpcManagerStartedLevel' after level download.");
                            LobbyGameStateController_HandleMenuRpcManagerStartedLevel.LobbyGameStateController.HandleMenuRpcManagerStartedLevel(LobbyGameStateController_HandleMenuRpcManagerStartedLevel.LastUserId, bmId, modifiers, startTime);
                        }
                        else
                            Plugin.Log?.Warn($"TryDownloadSong was unsuccessful.");
                        //MultiplayerLevelLoader.LoadLevel(bmId, modifiers, startTime);
                        LoadingLevelId = null;
                    });
                    return false;
                }
            }
            LoadingLevelId = null;
            return true;
        }

    }

    [HarmonyPatch(typeof(BeatmapLevelsModel), nameof(BeatmapLevelsModel.GetBeatmapLevelAsync),
    new Type[] { // List the Types of the method's parameters.
        typeof(string), typeof(CancellationToken) })]
    public class BeatmapLevelsModel_GetBeatmapLevelAsync
    {
        private static FieldAccessor<MultiplayerLevelLoader, IPreviewBeatmapLevel>.Accessor PreviewBeatmapLevel = FieldAccessor<MultiplayerLevelLoader, IPreviewBeatmapLevel>.GetAccessor("_previewBeatmapLevel");

        private static FieldAccessor<BeatmapLevelsModel, CustomLevelLoader>.Accessor CustomLevelLoader = FieldAccessor<BeatmapLevelsModel, CustomLevelLoader>.GetAccessor("_customLevelLoader");

        static bool Prefix(ref string levelID, ref CancellationToken cancellationToken, ref Task<BeatmapLevelsModel.GetBeatmapLevelResult> __result, BeatmapLevelsModel __instance)
        {
            //return true;
            if (!levelID.StartsWith("custom_level_") || SongCore.Loader.GetLevelById(levelID) != null)
                return true;
            Plugin.Log?.Info($"Attempting to download custom level...");
            TaskCompletionSource<BeatmapLevelsModel.GetBeatmapLevelResult> tcs = new TaskCompletionSource<BeatmapLevelsModel.GetBeatmapLevelResult>();
            __result = tcs.Task;
            tcs.Task.ContinueWith(r =>
            {
                if (r.Result.beatmapLevel != null)
                {
                    PreviewBeatmapLevel(ref MultiplayerLevelLoader_LoadLevel.MultiplayerLevelLoader) = r.Result.beatmapLevel;
                    Plugin.Log?.Info($"PreviewBeatmap set to {r.Result.beatmapLevel.songName}");
                }
                else
                    Plugin.Log?.Debug($"PreviewBeatmapLevel is null.");
            });
            TryDownloadSong(levelID, tcs, cancellationToken, __instance);
            return false;
        }

        public static async void TryDownloadSong(string levelId, TaskCompletionSource<BeatmapLevelsModel.GetBeatmapLevelResult> tcs, CancellationToken cancellationToken, BeatmapLevelsModel beatmapLevelsModel)
        {
            try
            {
                IPreviewBeatmapLevel beatmap = await Downloader.DownloadSong(levelId, cancellationToken);
                if (beatmap is CustomPreviewBeatmapLevel customLevel)
                {
                    Plugin.Log?.Debug($"Download was successful.");
                    IBeatmapLevel beatmapLevel = await CustomLevelLoader(ref beatmapLevelsModel).LoadCustomBeatmapLevelAsync(customLevel, cancellationToken);
                    UIHelper.RefreshUI();
                    tcs.TrySetResult(new BeatmapLevelsModel.GetBeatmapLevelResult(false, beatmapLevel));
                }
                else
                    Plugin.Log?.Error($"beatmap:{beatmap?.GetType().Name} is not an CustomPreviewBeatmapLevel");
            }
            catch (OperationCanceledException)
            {
                Plugin.Log?.Debug($"Download was canceled.");
                tcs.TrySetCanceled(cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"Error downloading beatmap '{levelId}': {ex.Message}");
                Plugin.Log?.Debug(ex);
            }
            tcs.TrySetResult(new BeatmapLevelsModel.GetBeatmapLevelResult(true, null));
            Plugin.Log?.Debug($"Download was unsuccessful.");
        }
    }


#if DEBUG
    [HarmonyPatch(typeof(SelectLevelCategoryViewController), nameof(SelectLevelCategoryViewController.selectedLevelCategory), MethodType.Getter)]
    public class TestThing
    {
        /// <summary>
        /// This code is run before the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        static bool Prefix(ref SelectLevelCategoryViewController.LevelCategoryInfo[] ____levelCategoryInfos, ref IconSegmentedControl ____levelFilterCategoryIconSegmentedControl)
        {
            var con = ____levelFilterCategoryIconSegmentedControl;
            var col = ____levelCategoryInfos;
            Plugin.Log?.Warn($"Selected cell: {con.selectedCellNumber} / {col.Length} ({con.NumberOfCells()})");
            if (con.selectedCellNumber < 0 || con.selectedCellNumber > col.Length)
                con.SelectCellWithNumber(0);
            foreach (var item in col)
            {
                Plugin.Log?.Warn($"{item.levelCategory}");
            }
            return true;
        }
    }
#endif
}
public class UIHelper : MonoBehaviour // This is needed because a static class cannot inherit from Monobehaviour
{
    public static void RefreshUI()
    {
        Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().FirstOrDefault().UpdateCustomSongs();
    }
}