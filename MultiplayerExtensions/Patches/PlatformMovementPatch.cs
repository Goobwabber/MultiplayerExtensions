using HarmonyLib;

namespace MultiplayerExtensions.Patches
{
    [HarmonyPatch]
    public class PlatformMovementPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerVerticalPlayerMovementManager), nameof(MultiplayerVerticalPlayerMovementManager.Update))]
        private static bool DisableVerticalPlayerMovement()
        {
            return !Plugin.Config.DisablePlatformMovement;
        }
    }
}
