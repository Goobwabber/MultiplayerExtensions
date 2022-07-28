using HarmonyLib;

namespace MultiplayerExtensions.Patches
{
    [HarmonyPatch]
    public class ResumeSpawningPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MultiplayerConnectedPlayerFacade), nameof(MultiplayerConnectedPlayerFacade.ResumeSpawning))]
        private static bool DisableAvatarRestrictions()
        {
            if (Plugin.Config.DisableMultiplayerObjects)
                return false;
            return true;
        }
    }
}
