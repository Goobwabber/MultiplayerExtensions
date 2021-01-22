using HarmonyLib;
using HMUI;
using IPA.Utilities;
using MultiplayerExtensions.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(CreateServerFormController), "formData", MethodType.Getter)]
    class IncreaseMaxPlayersClampPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].OperandIs(5))
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_R4, 10f);
                }
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(CreateServerFormController), "Setup", MethodType.Normal)]
    class IncreaseMaxPlayersPatch
    {
        static void Prefix(CreateServerFormController __instance)
        {
            FormattedFloatListSettingsController serverForm = __instance.GetField<FormattedFloatListSettingsController, CreateServerFormController>("_maxPlayersList");
            serverForm.values = new float[] { 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f };
        }
    }

    [HarmonyPatch(typeof(MultiplayerPlayerPlacement), "GetAngleBetweenPlayersWithEvenAdjustment", MethodType.Normal)]
    class PlayerPlacementAnglePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool flag = true;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4)
                {
                    flag = false;
                }

                if (flag)
                {
                    codes.RemoveAt(0);
                    i--;
                }
            }
            return codes.AsEnumerable();
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
