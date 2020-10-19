using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace MultiplayerExtensions.HarmonyPatches
{
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
                beatSaverWarning.GetComponent<RectTransform>().offsetMin = new Vector2(-23.5f, 100f);
                beatSaverWarning.GetComponent<RectTransform>().offsetMax = new Vector2(100f, -28f);
                beatSaverWarning.SetActive(false);
            }

            beatSaverWarning.SetActive(false);

            if (level.levelID.Contains("custom_level") && LobbyJoinPatch.IsMultiplayer)
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
}
