using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;

namespace MultiplayerExtensions.Patchers
{
    public class PlayerPositionPatcher : IAffinity
    {
        private readonly IMultiplayerSessionManager _sessionManager;
        private readonly Config _config;
        private readonly SiraLog _logger;

        internal PlayerPositionPatcher(
            IMultiplayerSessionManager sessionManager,
            Config config,
            SiraLog logger)
        {
            _sessionManager = sessionManager;
            _config = config;
            _logger = logger;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLayoutProvider), nameof(MultiplayerLayoutProvider.CalculateLayout))]
        private bool SoloEnvironmentLayout(ref MultiplayerPlayerLayout __result)
        {
            __result = MultiplayerPlayerLayout.Duel;
            return !_config.SoloEnvironment;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer))]
        private bool SoloEnvironmentAngle(int playerIndex, int localPlayerIndex, ref float __result)
        {
            __result = playerIndex - localPlayerIndex;
            return !_config.SoloEnvironment;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetPlayerWorldPosition))]
        private bool SoloEnvironmentPosition(float outerCirclePositionAngle, ref Vector3 __result)
        {
            var sortIndex = (int)outerCirclePositionAngle ;
            __result = new Vector3(sortIndex * 4f, 0, 0);
            return !_config.SoloEnvironment;
        }
    }
}
