using HarmonyLib;
using HMUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Reflection.Emit;
using UnityEngine.UI;
using System.Reflection;

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

    [HarmonyPatch(typeof(LightWithIdMonoBehaviour), "RegisterLight", MethodType.Normal)]
    internal class ShortCircuitPlatformLightRegistry
    {
        static bool Prefix(TubeBloomPrePassLightWithId __instance)
        {
            return !(__instance.transform.parent != null && __instance.transform.parent.name.Contains("LobbyAvatarPlace"));
        }
    }

    [HarmonyPatch(typeof(MultiplayerResultsPyramidView), nameof(MultiplayerResultsPyramidView.SetupResults), MethodType.Normal)]
    internal class MultiplayerResultsPyramidPatch
    {
        static void Prefix(ref IReadOnlyList<MultiplayerPlayerResultsData> resultsData)
        {
            resultsData = resultsData.Take(5).ToList();
        }
    }

    [HarmonyPatch(typeof(MultiplayerIntroAnimationController), nameof(MultiplayerIntroAnimationController.PlayIntroAnimation), MethodType.Normal)]
    internal class IntroAnimationPatch
    {
        private static PlayableDirector lastDirector = null!;
        internal static int targetIterations = 0;

        static void Prefix(MultiplayerIntroAnimationController __instance, ref bool ____bindingFinished, ref PlayableDirector ____introPlayableDirector, ref MultiplayerPlayersManager ____multiplayerPlayersManager)
        {
            lastDirector = ____introPlayableDirector;

            if (targetIterations == 0)
            {
                targetIterations = (int)Math.Floor((____multiplayerPlayersManager.allActiveAtGameStartPlayers.Count - 1) / 4f) + 1;
            }
            if (targetIterations != 1)
            {
                GameObject newPlayableGameObject = new GameObject();
                ____introPlayableDirector = newPlayableGameObject.AddComponent<PlayableDirector>();
                ____introPlayableDirector.playableAsset = lastDirector.playableAsset;
            }

            TimelineAsset mutedTimeline = (TimelineAsset)____introPlayableDirector.playableAsset;
            foreach (TrackAsset track in mutedTimeline.GetOutputTracks())
            {
                track.muted = track is AudioTrack && targetIterations != 1;
            }

            ____bindingFinished = false;
        }

        static void Postfix(MultiplayerIntroAnimationController __instance, float maxDesiredIntroAnimationDuration, Action onCompleted, ref PlayableDirector ____introPlayableDirector)
        {
            ____introPlayableDirector = lastDirector;
            targetIterations--;
            if (targetIterations != 0)
                __instance.PlayIntroAnimation(maxDesiredIntroAnimationDuration, onCompleted);
        }
    }

    [HarmonyPatch(typeof(MultiplayerPlayersManager), nameof(MultiplayerPlayersManager.allActiveAtGameStartPlayers), MethodType.Getter)]
    internal class AnimationPlayerCountPatch
    {
        static void Postfix(ref IReadOnlyList<IConnectedPlayer> __result)
        {
            StackTrace stackTrace = new StackTrace();
            string methodName = stackTrace.GetFrame(2).GetMethod().Name;
            if (methodName == "BindTimeline")
            {
                if (__result.Any(player => player.isMe))
                {
                    List<IConnectedPlayer> nonLocalPlayers = __result.Where(player => !player.isMe).ToList();
                    IConnectedPlayer localPlayer = __result.First(player => player.isMe);
                    __result = nonLocalPlayers.Skip((IntroAnimationPatch.targetIterations - 1) * 4).Take(4).ToList();
                    if (IntroAnimationPatch.targetIterations == 1)
                        __result = __result.AddItem(localPlayer).ToList();
                }
                else
                {
                    __result = __result.Skip((IntroAnimationPatch.targetIterations - 1) * 4).Take(4).ToList();
                }
            } 
            else if (methodName == "BindOutroTimeline")
            {
                __result = __result.Take(4).ToList();
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerOutroAnimationController), nameof(MultiplayerOutroAnimationController.BindRingsAndAudio), MethodType.Normal)]
    internal class AnimationRingCountPatch
    {
        static void Prefix(ref GameObject[] rings)
        {
            rings = rings.Take(5).ToArray();
        }
    }

    [HarmonyPatch(typeof(MultiplayerLobbyAvatarManager), nameof(MultiplayerLobbyAvatarManager.AddPlayer), MethodType.Normal)]
    internal class MultiplayerLobbyAvatarAddedPatch
    {
        static void Postfix(IConnectedPlayer connectedPlayer, MultiplayerLobbyAvatarManager __instance)
        {
            MPEvents.RaiseLobbyAvatarCreated(__instance, connectedPlayer);
        }
    }

    [HarmonyPatch(typeof(LobbySetupViewController), nameof(LobbySetupViewController.SetLobbyState), MethodType.Normal)]
    internal class EnableCancelButtonPatch
	{
        private static readonly MethodInfo _rootMethod = typeof(Selectable).GetProperty(nameof(Selectable.interactable)).GetSetMethod();
        private static readonly FieldInfo _cancelGameUnreadyButtonPrefab = typeof(LobbySetupViewController).GetField("_cancelGameUnreadyButton", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly MethodInfo _setInteractableAttacher = SymbolExtensions.GetMethodInfo(() => SetInteractableAttacher(null, false));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(_cancelGameUnreadyButtonPrefab))
                {
                    if (codes[i + 4].opcode == OpCodes.Callvirt && codes[i + 4].Calls(_rootMethod))
                    {
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _setInteractableAttacher);
                        codes[i + 4] = newCode;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        private static void SetInteractableAttacher(Selectable contract, bool value)
        {
            contract.interactable = !MPState.CurrentMasterServer.isOfficial ? true : value;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleLobbySetupViewControllerStartGameOrReady), MethodType.Normal)]
    internal class YeetPredictionsPatch
	{
        private static readonly MethodInfo _rootMethod = typeof(LobbyPlayerPermissionsModel).GetProperty(nameof(LobbyPlayerPermissionsModel.isPartyOwner)).GetGetMethod();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly MethodInfo _getIsPartyOwnerAttacher = SymbolExtensions.GetMethodInfo(() => GetIsPartyOwnerAttacher(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].OperandIs(_rootMethod))
                {
                    codes[i] = new CodeInstruction(OpCodes.Callvirt, _getIsPartyOwnerAttacher);
                }
            }

            return codes.AsEnumerable();
        }

        private static bool GetIsPartyOwnerAttacher(LobbyPlayerPermissionsModel contract)
		{
            return false;
		}
	}

    [HarmonyPatch(typeof(AvatarPoseRestrictions), nameof(AvatarPoseRestrictions.HandleAvatarPoseControllerPositionsWillBeSet), MethodType.Normal)]
    internal class DisableAvatarRestrictions
    {
        static bool Prefix(AvatarPoseRestrictions __instance, Vector3 headPosition, Vector3 leftHandPosition, Vector3 rightHandPosition, out Vector3 newHeadPosition, out Vector3 newLeftHandPosition, out Vector3 newRightHandPosition)
        {
            newHeadPosition = headPosition;
            newLeftHandPosition = __instance.LimitHandPositionRelativeToHead(leftHandPosition, headPosition);
            newRightHandPosition = __instance.LimitHandPositionRelativeToHead(rightHandPosition, headPosition);
            return false;
        }
    }
}
