using HarmonyLib;
using UnityEngine;

namespace MultiplayerExtensions.Patches
{
    public class PlayerPositionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerLayoutProvider), nameof(MultiplayerLayoutProvider.CalculateLayout))]
        private static bool SoloEnvironmentLayout(ref MultiplayerPlayerLayout __result)
        {
            __result = MultiplayerPlayerLayout.Duel;
            return !Plugin.Config.SoloEnvironment;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerConditionalActiveByLayout), nameof(MultiplayerConditionalActiveByLayout.Start))]
        private static void SoloEnvironmentLayoutConfirm(MultiplayerConditionalActiveByLayout __instance, MultiplayerLayoutProvider ____layoutProvider)
        {
            if (____layoutProvider.layout == MultiplayerPlayerLayout.NotDetermined)
                __instance.HandlePlayersLayoutWasCalculated(MultiplayerPlayerLayout.Duel, 2);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerConditionalActiveByLayout), nameof(MultiplayerConditionalActiveByLayout.HandlePlayersLayoutWasCalculated))]
        private static void SoloEnvironmentObjectDisable(ref MultiplayerPlayerLayout layout)
        {
            if (Plugin.Config.SoloEnvironment)
                layout = MultiplayerPlayerLayout.Duel;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer))]
        private static bool SoloEnvironmentAngle(int playerIndex, int localPlayerIndex, ref float __result)
        {
            __result = playerIndex - localPlayerIndex;
            return !Plugin.Config.SoloEnvironment;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetPlayerWorldPosition))]
        private bool SoloEnvironmentPosition(float outerCirclePositionAngle, ref Vector3 __result)
        {
            var sortIndex = (int)outerCirclePositionAngle ;
            __result = new Vector3(sortIndex * 4f, 0, 0);
            return !Plugin.Config.SoloEnvironment;
        }
    }
}
