using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using HMUI;
using IPA.Logging;
using IPA.Utilities;
using System.Windows.Forms;

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

    [HarmonyPatch(typeof(LevelCollectionViewController), "HandleLevelCollectionTableViewDidSelectLevel", MethodType.Normal)]
    public class LevelCollectionViewController_DidSelectLevel
    {
        private static GameObject beatSaverWarning;
        private static List<string> songsNotFound = new List<string>();

        /// <summary>
        /// Tells the user when they have selected a song that is not on BeatSaver.com.
        /// </summary>
        static bool Prefix(IPreviewBeatmapLevel level)
        {
            if (!beatSaverWarning)
            {
                var levelDetail = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First();
                beatSaverWarning = new GameObject("BeatSaverWarning", typeof(CurvedTextMeshPro));
                beatSaverWarning.transform.SetParent(levelDetail.transform, false);
                beatSaverWarning.GetComponent<CurvedTextMeshPro>().text = "Song not found on BeatSaver.com!";
                beatSaverWarning.GetComponent<CurvedTextMeshPro>().fontSize = 4;
                beatSaverWarning.GetComponent<CurvedTextMeshPro>().color = Color.red;
                beatSaverWarning.GetComponent<RectTransform>().offsetMin = new Vector2(-22.5f, 100f);
                beatSaverWarning.GetComponent<RectTransform>().offsetMax = new Vector2(100f, -28f);
                beatSaverWarning.SetActive(false);
            }

            beatSaverWarning.SetActive(false);

            if (level.levelID.Contains("custom_level"))
            {
                string levelHash = level.levelID.Replace("custom_level_", "");
                if (songsNotFound.Contains(levelHash))
                {
                    Plugin.Log?.Warn($"Could not find song '{levelHash}' on BeatSaver.");
                    beatSaverWarning.SetActive(true);
                    songsNotFound.Add(levelHash);
                    return true;
                }

                BeatSaverSharp.BeatSaver.Client.Hash(levelHash, CancellationToken.None).ContinueWith(r =>
                {
                    if (r.Result == null)
                    {
                        Plugin.Log?.Warn($"Could not find song '{levelHash}' on BeatSaver.");
                        beatSaverWarning.SetActive(true);
                        songsNotFound.Add(levelHash);
                    }
                    else
                    {
                        Plugin.Log?.Debug($"Selected song '{levelHash}' from BeatSaver.");
                    }
                });
                return true;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", MethodType.Getter)]
    public class EnableCustomLevelsPatch
    {
        public static bool Enabled;
        /// <summary>
        /// Overrides getter for <see cref="MultiplayerLevelSelectionFlowCoordinator.enableCustomLevels"/>
        /// </summary>
        static bool Prefix(ref bool __result)
        {
            Plugin.Log?.Debug($"CustomLevels are {(Enabled ? "enabled" : "disabled")}.");
            __result = Enabled;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "DidActivate",
        new Type[] { // List the Types of the method's parameters.
        typeof(bool), typeof(bool), typeof(bool) })]
    public class GameServerLobbyFlowCoordinator_DidActivate
    {
        /// <summary>
        /// Enables custom levels if GameServerLobbyFlowCoordinator.DidActivate is called.
        /// </summary>
        static void Prefix()
        {
            Plugin.Log?.Debug("Enabling CustomLevels");
            EnableCustomLevelsPatch.Enabled = true;
        }
    }

    [HarmonyPatch(typeof(QuickPlayLobbyFlowCoordinator), "DidActivate",
        new Type[] { // List the Types of the method's parameters.
        typeof(bool), typeof(bool), typeof(bool) })]
    public class QuickPlayLobbyFlowCoordinator_DidActivate
    {
        /// <summary>
        /// Disables custom levels if QuickPlayLobbyFlowCoordinator.DidActivate is called.
        /// </summary>
        static void Prefix()
        {
            Plugin.Log?.Debug("Disabling CustomLevels");
            EnableCustomLevelsPatch.Enabled = false;
        }
    }
}