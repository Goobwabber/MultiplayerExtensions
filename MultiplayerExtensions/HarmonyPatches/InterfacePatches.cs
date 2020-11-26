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
    [HarmonyPatch(typeof(GameServerPlayerTableCell), "SetData", MethodType.Normal)]
    public class GameServerPlayerTablePatch
    {
        private static Color green = new Color(0f, 1f, 0f, 1f);
        private static Color yellow = new Color(0.125f, 0.75f, 1f, 1f);
        private static Color red = new Color(1f, 0f, 0f, 1f);
        private static Color normal = new Color(0.125f, 0.75f, 1f, 0.1f);

        private static Dictionary<string, GameServerPlayerTableCell> cells = new Dictionary<string, GameServerPlayerTableCell>();
        private static Dictionary<string, ILobbyPlayerDataModel> models = new Dictionary<string, ILobbyPlayerDataModel>();

        static void Postfix(IConnectedPlayer connectedPlayer, ILobbyPlayerDataModel playerDataModel, GameServerPlayerTableCell __instance)
        {
            if (!cells.ContainsValue(__instance))
            {
                cells[connectedPlayer.userId] = __instance;
                models[connectedPlayer.userId] = playerDataModel;
            }

            Update(connectedPlayer, playerDataModel, __instance);
        }

        public static async void Update(IConnectedPlayer connectedPlayer, ILobbyPlayerDataModel playerDataModel, GameServerPlayerTableCell __instance)
        {
            Image background = __instance.GetField<Image, GameServerPlayerTableCell>("_localPlayerBackgroundImage");
            if (playerDataModel.beatmapLevel != null)
            {
                background.enabled = true;
                PreviewBeatmapStub preview = await PreviewBeatmapManager.CreatePreview(playerDataModel.beatmapLevel.levelID);

                Color color = connectedPlayer.HasState("beatmap_downloaded") ? green : await preview.isDownloadable ? yellow : red;
                color.a = connectedPlayer.isMe ? 0.4f : 0.1f;

                HMMainThreadDispatcher.instance.Enqueue(() => background.color = color);
            }
            else
            {
                HMMainThreadDispatcher.instance.Enqueue(() => background.color = normal);
            }
        }

        public static void SetLoading(IConnectedPlayer player)
        {
            GameServerPlayerTableCell cell = cells[player.userId];
            CurvedTextMeshPro emptySuggestion = cell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_emptySuggestedLevelText");
            CurvedTextMeshPro suggestion = cell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_suggestedLevelText");

            suggestion.gameObject.SetActive(false);
            emptySuggestion.gameObject.SetActive(true);

            emptySuggestion.text = "Loading...";
        }

        public static void Update(IConnectedPlayer player)
        {
            if (cells.TryGetValue(player.userId, out GameServerPlayerTableCell cell))
            {
                ILobbyPlayerDataModel model = models[player.userId];
                Update(player, model, cell);
            }
            else
                Plugin.Log?.Debug($"Tried to update UI for player '{player.userName} ({player.userId})', but there wasn't an associated TableCell.");
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
