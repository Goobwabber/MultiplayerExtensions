using BS_Utils.Utilities;
using HarmonyLib;
using MultiplayerExtensions.Avatars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MenuInstaller), "InstallBindings", MethodType.Normal)]
    class LobbyAvatarPatch
    {
        static void Prefix(MenuInstaller __instance)
        {
            if (IPA.Loader.PluginManager.GetPluginFromId("CustomAvatar") != null)
            {
                MultiplayerLobbyAvatarController multiplayerLobbyAvatarController = __instance.GetField<MultiplayerLobbyAvatarController>("_multiplayerLobbyAvatarControllerPrefab");
                if (!multiplayerLobbyAvatarController.gameObject.TryGetComponent<CustomAvatarController>(out CustomAvatarController controller))
                {
                    multiplayerLobbyAvatarController.gameObject.AddComponent<CustomAvatarController>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerCoreInstaller), "InstallBindings", MethodType.Normal)]
    class GameAvatarPatch
    {
        static void Postfix(MultiplayerCoreInstaller __instance)
        {
            if (IPA.Loader.PluginManager.GetPluginFromId("CustomAvatar") != null)
            {
                MultiplayerPlayersManager playersManager = Resources.FindObjectsOfTypeAll<MultiplayerPlayersManager>().First();

                MultiplayerConnectedPlayerFacade playerFacade = playersManager.GetField<MultiplayerConnectedPlayerFacade>("_connectedPlayerControllerPrefab");
                MultiplayerConnectedPlayerFacade duelPlayerFacade = playersManager.GetField<MultiplayerConnectedPlayerFacade>("_connectedPlayerDuelControllerPrefab");
                SetupPoseController(playerFacade);
                SetupPoseController(duelPlayerFacade);
            }
        }

        static void SetupPoseController(MultiplayerConnectedPlayerFacade playerFacade)
        {
            MultiplayerAvatarPoseController multiplayerPoseController = playerFacade.GetComponentsInChildren<MultiplayerAvatarPoseController>().First();
            if (!multiplayerPoseController.gameObject.TryGetComponent<CustomAvatarController>(out CustomAvatarController controller))
            {
                multiplayerPoseController.gameObject.AddComponent<CustomAvatarController>();
            }
        }
    }
}
