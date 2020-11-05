using BS_Utils.Utilities;
using HarmonyLib;
using MultiplayerExtensions.Avatars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MenuInstaller), "InstallBindings", MethodType.Normal)]
    class AvatarPatch
    {
        static void Prefix(MenuInstaller __instance)
        {
            MultiplayerLobbyAvatarController multiplayerLobbyAvatarController = __instance.GetField<MultiplayerLobbyAvatarController>("_multiplayerLobbyAvatarControllerPrefab");
            multiplayerLobbyAvatarController.gameObject.AddComponent<CustomLobbyAvatarController>();
        }
    }
}
