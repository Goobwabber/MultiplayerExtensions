using HarmonyLib;
using HMUI;
using IPA.Utilities;
using MultiplayerExtensions.Beatmaps;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(GameServerPlayersTableView), "SetData", MethodType.Normal)]
    public class GameServerPlayerTablePatch
    {
        private static Color green = new Color(0f, 1f, 0f, 1f);
        private static Color yellow = new Color(0.125f, 0.75f, 1f, 1f);
        private static Color red = new Color(1f, 0f, 0f, 1f);
        private static Color normal = new Color(0.125f, 0.75f, 1f, 0.1f);

        static void Postfix(List<IConnectedPlayer> sortedPlayers, ILobbyPlayersDataModel lobbyPlayersDataModel, GameServerPlayersTableView __instance)
        {
            IPreviewBeatmapLevel hostBeatmap = lobbyPlayersDataModel.GetPlayerBeatmapLevel(lobbyPlayersDataModel.hostUserId);
            if (hostBeatmap != null && hostBeatmap is PreviewBeatmapStub hostBeatmapStub)
            {
                TableView tableView = __instance.GetField<TableView, GameServerPlayersTableView>("_tableView");
                foreach (TableCell cell in tableView.visibleCells)
                {
                    if (cell is GameServerPlayerTableCell playerCell)
                    {
                        Image background = playerCell.GetField<Image, GameServerPlayerTableCell>("_localPlayerBackgroundImage");
                        CurvedTextMeshPro emptySuggestion = playerCell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_emptySuggestedLevelText");
                        CurvedTextMeshPro suggestion = playerCell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_suggestedLevelText");
                        IConnectedPlayer player = sortedPlayers[playerCell.idx];
                        Color backgroundColor = new Color();

                        if (player.isConnectionOwner)
                        {
                            suggestion.gameObject.SetActive(false);
                            emptySuggestion.gameObject.SetActive(true);
                            emptySuggestion.text = "Loading...";
                            hostBeatmapStub.isDownloadable.ContinueWith(r =>
                            {
                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    suggestion.gameObject.SetActive(true);
                                    emptySuggestion.gameObject.SetActive(false);
                                });
                            });
                        }

                        background.enabled = true;
                        if (player.HasState("beatmap_downloaded") || player.HasState("start_primed"))
                        {
                            backgroundColor = green;
                            backgroundColor.a = player.isMe ? 0.4f : 0.1f;
                            background.color = backgroundColor;
                        }
                        else
                        {
                            hostBeatmapStub.isDownloadable.ContinueWith(r =>
                            {
                                bool downloadable = r.Result;
                                backgroundColor = downloadable ? yellow : red;
                                backgroundColor.a = player.isMe ? 0.4f : 0.1f;
                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    background.color = backgroundColor;
                                });
                            });
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerBigAvatarAnimator), "InitIfNeeded", MethodType.Normal)]
    class MultiplayerBigAvatarAnimator_Init
    {
        static void Postfix(MultiplayerBigAvatarAnimator __instance)
        {
            Plugin.Log?.Debug($"{(Plugin.Config.Hologram ? "Enabled" : "Disabled")} hologram.");
            __instance.gameObject.SetActive(Plugin.Config.Hologram);
        }
    }

    [HarmonyPatch(typeof(CoreGameHUDController), "Start", MethodType.Normal)]
    class CoreGameHUDController_Start
    {
        static void Postfix(CoreGameHUDController __instance)
        {
            if (LobbyJoinPatch.IsMultiplayer && Plugin.Config.VerticalHUD)
            {
                Plugin.Log?.Debug("Setting up multiplayer HUD");
                GameEnergyUIPanel gameEnergyUI = __instance.transform.GetComponentInChildren<GameEnergyUIPanel>();

                __instance.transform.position = new Vector3(0f, 0f, 10f);
                __instance.transform.eulerAngles = new Vector3(270f, 0f, 0f);

                if (gameEnergyUI != null)
                {
                    gameEnergyUI.transform.localPosition = new Vector3(0f, 4f, 0f);
                    gameEnergyUI.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
                }

                if (Plugin.Config.SingleplayerHUD)
                {
                    Transform comboPanel = __instance.transform.Find("ComboPanel");
                    Transform scoreCanvas = __instance.transform.Find("ScoreCanvas");
                    Transform multiplierCanvas = __instance.transform.Find("MultiplierCanvas");
                    Transform songProgressCanvas = __instance.transform.Find("SongProgressCanvas");

                    if (!__instance.transform.Find("LeftPanel"))
                    {
                        GameObject leftPanel = new GameObject();
                        GameObject rightPanel = new GameObject();
                        leftPanel.name = "LeftPanel";
                        rightPanel.name = "RightPanel";
                        leftPanel.transform.parent = __instance.transform;
                        rightPanel.transform.parent = __instance.transform;
                        leftPanel.transform.localPosition = new Vector3(-2.5f, 0f, 1f);
                        rightPanel.transform.localPosition = new Vector3(2.5f, 0f, 1f);

                        comboPanel.transform.parent = leftPanel.transform;
                        scoreCanvas.transform.parent = leftPanel.transform;
                        multiplierCanvas.transform.parent = rightPanel.transform;
                        songProgressCanvas.transform.parent = rightPanel.transform;

                        comboPanel.transform.localPosition = new Vector3(0f, 0f, 0f);
                        scoreCanvas.transform.localPosition = new Vector3(0f, -1.1f, 0f);
                        multiplierCanvas.transform.localPosition = new Vector3(0f, 0f, 0f);
                        songProgressCanvas.transform.localPosition = new Vector3(0f, -1.1f, 0f);

                        comboPanel.transform.SetParent(__instance.transform, true);
                        scoreCanvas.transform.SetParent(__instance.transform, true);
                        multiplierCanvas.transform.SetParent(__instance.transform, true);
                        songProgressCanvas.transform.SetParent(__instance.transform, true);

                        CurvedTextMeshPro[]? scorePanels = scoreCanvas.GetComponentsInChildren<CurvedTextMeshPro>();
                        foreach (CurvedTextMeshPro panel in scorePanels)
                        {
                            panel.enabled = true;
                        }
                    }
                }
            }
        }
    }
}
