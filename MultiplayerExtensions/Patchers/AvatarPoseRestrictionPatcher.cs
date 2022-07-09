using SiraUtil.Affinity;
using UnityEngine;

namespace MultiplayerExtensions.Patchers
{
    public class AvatarPoseRestrictionPatcher : IAffinity
    {
        private readonly Config _config;

        internal AvatarPoseRestrictionPatcher(
            Config config)
        {
            _config = config;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(AvatarPoseRestrictions), nameof(AvatarPoseRestrictions.HandleAvatarPoseControllerPositionsWillBeSet))]
        private bool DisableAvatarRestrictions(AvatarPoseRestrictions __instance, Vector3 headPosition, Vector3 leftHandPosition, Vector3 rightHandPosition, out Vector3 newHeadPosition, out Vector3 newLeftHandPosition, out Vector3 newRightHandPosition)
        {
            newHeadPosition = headPosition;
            newLeftHandPosition = leftHandPosition;
            newRightHandPosition = rightHandPosition;
            if (!_config.DisableAvatarConstraints)
                return true;
            newLeftHandPosition = __instance.LimitHandPositionRelativeToHead(leftHandPosition, headPosition);
            newRightHandPosition = __instance.LimitHandPositionRelativeToHead(rightHandPosition, headPosition);
            return false;
        }
    }
}
