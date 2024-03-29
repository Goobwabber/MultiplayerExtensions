﻿using HarmonyLib;
using SiraUtil.Affinity;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerExtensions.Patchers
{
    [HarmonyPatch]
    public class PlayerPositionPatcher : IAffinity
    {
        private readonly Config _config;

        internal PlayerPositionPatcher(
            Config config)
        {
            _config = config;
        }

        // these are affinity patches because they only apply to one container
        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLayoutProvider), nameof(MultiplayerLayoutProvider.CalculateLayout))]
        private bool SideBySideLayout(ref MultiplayerPlayerLayout __result)
        {;
            __result = MultiplayerPlayerLayout.Duel;
            return !_config.SideBySide;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerConditionalActiveByLayout), nameof(MultiplayerConditionalActiveByLayout.Start))]
        private static void SideBySideLayoutConfirm(MultiplayerConditionalActiveByLayout __instance, MultiplayerLayoutProvider ____layoutProvider)
        {
            if (!Plugin.Config.SideBySide)
                return;
            if (____layoutProvider.layout == MultiplayerPlayerLayout.NotDetermined)
                __instance.HandlePlayersLayoutWasCalculated(MultiplayerPlayerLayout.Duel, 2);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerConditionalActiveByLayout), nameof(MultiplayerConditionalActiveByLayout.HandlePlayersLayoutWasCalculated))]
        private static void SideBySideObjectDisable(ref MultiplayerPlayerLayout layout)
        {
            if (Plugin.Config.SideBySide)
                layout = MultiplayerPlayerLayout.Duel;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer))]
        private bool SideBySideAngle(int playerIndex, int localPlayerIndex, ref float __result)
        {
            __result = (playerIndex - localPlayerIndex) * 0.01f;
            return !_config.SideBySide;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetPlayerWorldPosition))]
        private bool SoloEnvironmentPosition(float outerCirclePositionAngle, ref Vector3 __result)
        {
            var sortIndex = outerCirclePositionAngle ;
            __result = new Vector3(sortIndex * 100f * _config.SideBySideDistance, 0, 0);
            return !_config.SideBySide;
        }
    }
}
