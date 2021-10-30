using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using HMUI;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), nameof(MultiplayerModeSelectionFlowCoordinator.HandleJoinQuickPlayViewControllerDidFinish), MethodType.Normal)]
    internal class MultiplayerModeSelectionFlowCoordinatorPatch
    {
        internal static bool skipCheck = false;
        static bool Prefix(MultiplayerModeSelectionFlowCoordinator __instance, bool success, JoinQuickPlayViewController ____joinQuickPlayViewController, SimpleDialogPromptViewController ____simpleDialogPromptViewController, SongPackMaskModelSO ____songPackMaskModel)
        {
            string levelPackName = ____joinQuickPlayViewController.multiplayerModeSettings.quickPlaySongPackMaskSerializedName;
            Plugin.Log?.Debug(levelPackName);
            if (success && ____songPackMaskModel.ToSongPackMask(levelPackName).Contains("custom_levelpack_CustomLevels") && !skipCheck)
            {
                ____simpleDialogPromptViewController.Init(
                    "Custom Song Quickplay",
                    "<color=#EB4949>This category includes songs of varying difficulty.\nIt may be more enjoyable to play in a private lobby with friends.",
                    "Continue",
                    "Cancel",
                    delegate (int btnId)
                    {
                        switch (btnId)
                        {
                            default:
                            case 0: // Continue
                                skipCheck = true;
                                __instance.HandleJoinQuickPlayViewControllerDidFinish(true);
                                break;
                            case 1: // Cancel
                                __instance.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
                                    ____joinQuickPlayViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical
                                });
                                break;
                        }
                    }
                    );
                __instance.InvokeMethod<object, MultiplayerModeSelectionFlowCoordinator>("ReplaceTopViewController", new object[] {
                                    ____simpleDialogPromptViewController, null, ViewController.AnimationType.In, ViewController.AnimationDirection.Vertical
                                });
                return false;
            }
            skipCheck = false;
            return true;
        }
    }
}
