using HarmonyLib;
using HMUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerLobbyController), nameof(MultiplayerLobbyController.ActivateMultiplayerLobby), MethodType.Normal)]
    internal class LobbyEnvironmentLoadPatch
    {
        static void Prefix(ref float ____innerCircleRadius, ref float ____minOuterCircleRadius)
        {
            ____innerCircleRadius = 1f;
            ____minOuterCircleRadius = 4.4f;
        }

        static void Postfix(MultiplayerLobbyController __instance)
        {
            MPEvents.RaiseLobbyEnvironmentLoaded(__instance);
        }
    }

    [HarmonyPatch(typeof(MultiplayerBigAvatarAnimator), nameof(MultiplayerBigAvatarAnimator.InitIfNeeded), MethodType.Normal)]
    internal class MultiplayerBigAvatarAnimator_Init
    {
        static void Postfix(MultiplayerBigAvatarAnimator __instance)
        {
            Plugin.Log?.Debug($"{(Plugin.Config.Hologram ? "Enabled" : "Disabled")} hologram.");
            __instance.gameObject.SetActive(Plugin.Config.Hologram);
        }
    }

    [HarmonyPatch(typeof(CoreGameHUDController), nameof(CoreGameHUDController.Start), MethodType.Normal)]
    internal class CoreGameHUDController_Start
    {
        static void Postfix(CoreGameHUDController __instance, ref GameObject ____songProgressPanelGO, ref GameObject ____energyPanelGO)
        {
            if (MPState.CurrentGameType != MultiplayerGameType.None && Plugin.Config.VerticalHUD)
            {
                Plugin.Log?.Debug("Setting up multiplayer HUD");

                __instance.transform.position = new Vector3(0f, 0f, 10f);
                __instance.transform.eulerAngles = new Vector3(270f, 0f, 0f);

                ____energyPanelGO.transform.localPosition = new Vector3(0f, 4f, 0f);
                ____energyPanelGO.transform.localEulerAngles = new Vector3(270f, 0f, 0f);

                if (Plugin.Config.SingleplayerHUD && !__instance.transform.Find("LeftPanel"))
                {
                    Transform comboPanel = __instance.transform.Find("ComboPanel");
                    Transform scoreCanvas = __instance.transform.Find("ScoreCanvas");
                    Transform multiplierCanvas = __instance.transform.Find("MultiplierCanvas");

                    GameObject leftPanel = new GameObject();
                    GameObject rightPanel = new GameObject();
                    leftPanel.name = "LeftPanel";
                    rightPanel.name = "RightPanel";
                    leftPanel.transform.parent = __instance.transform;
                    rightPanel.transform.parent = __instance.transform;
                    leftPanel.transform.localPosition = new Vector3(-2.5f, 0f, 1f);
                    rightPanel.transform.localPosition = new Vector3(2.5f, 0f, 1f);

                    ____songProgressPanelGO.transform.SetParent(rightPanel.transform, true);
                    ____songProgressPanelGO.transform.localPosition = new Vector3(0f, -1.1f, 0f);
                    ____songProgressPanelGO.transform.SetParent(__instance.transform, true);

                    multiplierCanvas.transform.SetParent(rightPanel.transform, true);
                    multiplierCanvas.transform.localPosition = new Vector3(0f, 0f, 0f);
                    multiplierCanvas.transform.SetParent(__instance.transform, true);

                    comboPanel.transform.SetParent(leftPanel.transform, true);
                    comboPanel.transform.localPosition = new Vector3(0f, 0f, 0f);
                    comboPanel.transform.SetParent(__instance.transform, true);

                    scoreCanvas.transform.SetParent(leftPanel.transform, true);
                    scoreCanvas.transform.localPosition = new Vector3(0f, -1.1f, 0f);
                    scoreCanvas.transform.SetParent(__instance.transform, true);

                    foreach (CurvedTextMeshPro panel in scoreCanvas.GetComponentsInChildren<CurvedTextMeshPro>())
                    {
                        panel.enabled = true;
                    }
                }
            }
        }
    }
}
