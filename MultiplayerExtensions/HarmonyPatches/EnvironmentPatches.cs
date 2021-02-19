using HarmonyLib;
using HMUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerLobbyController), "ActivateMultiplayerLobby", MethodType.Normal)]
    internal class LobbyEnvironmentLoadPatch
    {
        static void Postfix(MultiplayerLobbyController __instance)
        {
            MPEvents.RaiseLobbyEnvironmentLoaded(__instance);
        }
    }

    [HarmonyPatch(typeof(MultiplayerBigAvatarAnimator), "InitIfNeeded", MethodType.Normal)]
    internal class MultiplayerBigAvatarAnimator_Init
    {
        static void Postfix(MultiplayerBigAvatarAnimator __instance)
        {
            Plugin.Log?.Debug($"{(Plugin.Config.Hologram ? "Enabled" : "Disabled")} hologram.");
            __instance.gameObject.SetActive(Plugin.Config.Hologram);
        }
    }

    [HarmonyPatch(typeof(MultiplayerLocalActiveLevelFailController), nameof(MultiplayerLocalActiveLevelFailController.HandlePlayerDidFinish), MethodType.Normal)]
    internal class SpectateOnFinishPatch
    {
        static void Postfix(LevelCompletionResults levelCompletionResults, ref MultiplayerPlayersManager ____multiplayerPlayersManager)
        {
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                ____multiplayerPlayersManager.StartCoroutine(SetLocalPlayerInactive(____multiplayerPlayersManager));
            }
            
        }

        static IEnumerator SetLocalPlayerInactive(MultiplayerPlayersManager multiplayerPlayersManager)
        {
            yield return new WaitForSeconds(2f);
            yield return multiplayerPlayersManager.StartCoroutine(multiplayerPlayersManager.SwitchLocalPlayerToInactiveCoroutine());
            yield break;
        }
    }

    [HarmonyPatch(typeof(MultiplayerOutroAnimationController), nameof(MultiplayerOutroAnimationController.PlayOutroAnimation), MethodType.Normal)]
    internal class CancelSpectateAnimationPatch
    {
        static void Prefix(ref float ____startDelay, ref MultiplayerPlayersManager ____multiplayerPlayersManager)
        {
            ____startDelay = 1f;
            ____multiplayerPlayersManager.StopAllCoroutines();
        }
    }

    [HarmonyPatch(typeof(MultiplayerLevelFinishedController), nameof(MultiplayerLevelFinishedController.HandlePlayerDidFinish), MethodType.Normal)]
    internal class CancelResultsAnimationPatch
    {
        static bool Prefix(LevelCompletionResults levelCompletionResults, ref IGameplayRpcManager ____rpcManager, ref LevelCompletionResults ____localPlayerResults, ref Dictionary<string, LevelCompletionResults> ____otherPlayersCompletionResults, ref IMultiplayerSessionManager ____multiplayerSessionManager, ref MultiplayerLevelFinishedController __instance)
        {
            LevelCompletionResults localPlayerResults = levelCompletionResults;
            if (localPlayerResults == null)
                localPlayerResults = new LevelCompletionResults(LevelCompletionResults.LevelEndAction.MultiplayerInactive);
            ____rpcManager.LevelFinished(localPlayerResults);
            ____localPlayerResults = localPlayerResults;

            bool allPlayersFinished = true;
            foreach (IConnectedPlayer player in ____multiplayerSessionManager.connectedPlayers)
            {
                allPlayersFinished = allPlayersFinished && (!player.IsActiveOrFinished() || ____otherPlayersCompletionResults.ContainsKey(player.userId));
            }
            if (allPlayersFinished)
                __instance.StartCoroutine(StartLevelFinished(__instance, levelCompletionResults));
            return false;
        }

        static IEnumerator StartLevelFinished(MultiplayerLevelFinishedController instance, LevelCompletionResults localPlayerResults)
        {
            yield return new WaitForSeconds(0.1f);
            yield return instance.StartCoroutine(instance.StartLevelFinished(localPlayerResults));
            yield break;
        }
    }

    [HarmonyPatch(typeof(MultiplayerLevelFinishedController), nameof(MultiplayerLevelFinishedController.HandleRpcLevelFinished), MethodType.Normal)]
    internal class InvokeResultsAnimationPatch
    {
        static bool Postfix(string userId, LevelCompletionResults results, ref LevelCompletionResults ____localPlayerResults, ref Dictionary<string, LevelCompletionResults> ____otherPlayersCompletionResults, ref IMultiplayerSessionManager ____multiplayerSessionManager, ref MultiplayerLevelFinishedController __instance)
        {
            if (____otherPlayersCompletionResults.ContainsKey(userId))
                return false;

            ____otherPlayersCompletionResults[userId] = results;

            bool allPlayersFinished = ____localPlayerResults != null;
            foreach (IConnectedPlayer player in ____multiplayerSessionManager.connectedPlayers)
            {
                allPlayersFinished = allPlayersFinished && (!player.IsActiveOrFinished() || ____otherPlayersCompletionResults.ContainsKey(player.userId));
            }
            if (allPlayersFinished)
                __instance.StartCoroutine(StartLevelFinished(__instance, ____localPlayerResults!));
            return false;
        }

        static IEnumerator StartLevelFinished(MultiplayerLevelFinishedController instance, LevelCompletionResults localPlayerResults)
        {
            yield return new WaitForSeconds(0.1f);
            yield return instance.StartCoroutine(instance.StartLevelFinished(localPlayerResults));
            yield break;
        }
    }

    [HarmonyPatch(typeof(CoreGameHUDController), "Start", MethodType.Normal)]
    internal class CoreGameHUDController_Start
    {
        static void Postfix(CoreGameHUDController __instance)
        {
            if (MPState.CurrentGameType != MultiplayerGameType.None && Plugin.Config.VerticalHUD)
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
