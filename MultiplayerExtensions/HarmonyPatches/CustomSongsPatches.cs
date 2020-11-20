using HarmonyLib;
using IPA.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
#if DEBUG
using System.Collections.Generic;
#endif
/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace MultiplayerExtensions.HarmonyPatches
{
#if DEBUG
    [HarmonyPatch(typeof(SongPackMasksModel), MethodType.Constructor,
        new Type[] { // List the Types of the method's parameters.
        typeof(BeatmapLevelsModel) })]
    public class SongPackMasksModel_Constructor
    {
        /// <summary>
        /// Adds a level pack selection to Quick Play's picker. Unfortunately, the server doesn't allow custom songs to be played in Quick Play.
        /// Left here for testing.
        /// </summary>
        static void Postfix(SongPackMasksModel __instance, ref BeatmapLevelsModel beatmapLevelsModel, ref List<Tuple<SongPackMask, string>> ____songPackMaskData)
        {
            SongPackMask customs = new SongPackMask("custom_levelpack_CustomLevels");
            ____songPackMaskData.Add(customs, "Custom");
        }
    }
#endif

    [HarmonyPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", MethodType.Getter)]
    public class EnableCustomLevelsPatch
    {
        /// <summary>
        /// Overrides getter for <see cref="MultiplayerLevelSelectionFlowCoordinator.enableCustomLevels"/>
        /// </summary>
        static bool Prefix(ref bool __result)
        {
            Plugin.Log?.Debug($"CustomLevels are {(LobbyJoinPatch.IsPrivate ? "enabled" : "disabled")}.");
            __result = LobbyJoinPatch.IsPrivate && Plugin.Config.CustomSongs;
            return false;
        }
    }

    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), "connectionType", MethodType.Setter)]
    class LobbyJoinPatch
    {
        public static MultiplayerLobbyConnectionController.LobbyConnectionType ConnectionType;

        public static bool IsPrivate { get { return ConnectionType != MultiplayerLobbyConnectionController.LobbyConnectionType.QuickPlay || false; } }
        public static bool IsHost { get { return ConnectionType == MultiplayerLobbyConnectionController.LobbyConnectionType.PartyHost || false; } }
        public static bool IsMultiplayer { get { return ConnectionType != MultiplayerLobbyConnectionController.LobbyConnectionType.None || false; } }

        /// <summary>
        /// Gets the current lobby type.
        /// </summary>
        static void Prefix(MultiplayerLobbyConnectionController __instance)
        {
            ConnectionType = __instance.GetProperty<MultiplayerLobbyConnectionController.LobbyConnectionType, MultiplayerLobbyConnectionController>("connectionType");
            Plugin.Log?.Debug($"Joining a {ConnectionType} lobby.");
        }
    }

    [HarmonyPatch(typeof(MultiplayerLevelLoader), nameof(MultiplayerLevelLoader.LoadLevel),
    new Type[] { typeof(BeatmapIdentifierNetSerializable), typeof(GameplayModifiers), typeof(float) })]
    public class LoadLevelPatch
    {
        public static MultiplayerLevelLoader? MultiplayerLevelLoader;
        public static BeatmapIdentifierNetSerializable? bmId;
        public static GameplayModifiers? modifiers;
        public static float? startTime;

        public static readonly string CustomLevelPrefix = "custom_level_";
        private static string? LoadingLevelId;

        static bool Prefix(ref BeatmapIdentifierNetSerializable beatmapId, ref GameplayModifiers gameplayModifiers, ref float initialStartTime, MultiplayerLevelLoader __instance)
        {
            string? levelId = beatmapId.levelID;
            if (SongCore.Loader.GetLevelById(levelId) != null)
            {
                if (LoadingLevelId != levelId)
                    Plugin.Log?.Debug($"Level with ID '{levelId}' already exists."); // Don't log if LoadLevel was called when a download finished.
                LoadingLevelId = null;
                return true;
            }
            string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
            if (hash == null)
            {
                Plugin.Log?.Info($"Could not get a hash from beatmap with LevelId {beatmapId.levelID}");
                LoadingLevelId = null;
                return true;
            }
            if (Downloader.TryGetDownload(levelId, out _))
            {
                Plugin.Log?.Debug($"Download for '{levelId}' is already in progress.");
                return false;
            }

            MultiplayerLevelLoader = __instance;
            bmId = beatmapId;
            modifiers = gameplayModifiers;
            startTime = initialStartTime;

            if (LoadingLevelId == null || LoadingLevelId != levelId)
            {
                LoadingLevelId = levelId;

                Plugin.Log?.Debug($"Attempting to download level with ID '{levelId}'...");
                Task? downloadTask = Downloader.TryDownloadSong(levelId, null, CancellationToken.None).ContinueWith(b =>
                {
                    try
                    {
                        IPreviewBeatmapLevel? level = b.Result;
                        if (level != null)
                        {
                            Plugin.Log?.Debug($"Level with ID '{levelId}' was downloaded successfully.");
                            MultiplayerLevelLoader.LoadLevel(bmId, modifiers, startTime);
                        }
                        else
                            Plugin.Log?.Warn($"TryDownloadSong was unsuccessful.");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log?.Warn($"Error in TryDownloadSong continuation: {ex.Message}");
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

        static async void DownloadSong()
        {
            IPreviewBeatmapLevel beatmap
        }
    }
}
