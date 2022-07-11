using HarmonyLib;
using UnityEngine;

namespace MultiplayerExtensions.Patches
{
    [HarmonyPatch]
    public class AvatarPoseRestrictionPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarPoseRestrictions), nameof(AvatarPoseRestrictions.HandleAvatarPoseControllerPositionsWillBeSet))]
        private static bool DisableAvatarRestrictions(AvatarPoseRestrictions __instance, Vector3 headPosition, Vector3 leftHandPosition, Vector3 rightHandPosition, out Vector3 newHeadPosition, out Vector3 newLeftHandPosition, out Vector3 newRightHandPosition)
        {
            newHeadPosition = headPosition;
            newLeftHandPosition = leftHandPosition;
            newRightHandPosition = rightHandPosition;
            if (!Plugin.Config.DisableAvatarConstraints)
                return true;
            newLeftHandPosition = __instance.LimitHandPositionRelativeToHead(leftHandPosition, headPosition);
            newRightHandPosition = __instance.LimitHandPositionRelativeToHead(rightHandPosition, headPosition);
            return false;
        }
    }
}
