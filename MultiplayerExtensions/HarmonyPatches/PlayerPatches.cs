using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;

namespace MultiplayerExtensions.HarmonyPatches
{
    //[HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerConnected", MethodType.Normal)]
    class MultiplayerSessionManager_HandlePlayerConnected
    {
        static void Postfix(IConnectedPlayer player, MultiplayerSessionManager __instance)
        {
            if (player.HasState("modded"))
            {
                Plugin.Log?.Debug($"Player {player.userName} is modded.");
            }
            else
            {
                Plugin.Log?.Debug($"Player {player.userName} is not modded.");
                if (LobbyJoinPatch.IsHost && Plugin.Config.EnforceMods)
                {
                    Plugin.Log?.Debug("Kicking player due to missing 'modded' state.");
                    var connectedPlayerManager = __instance.GetField<ConnectedPlayerManager, MultiplayerSessionManager>("_connectedPlayerManager");
                    connectedPlayerManager.KickPlayer(player.userId, DisconnectedReason.Kicked);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerSessionManager), "HandlePlayerStateChanged", MethodType.Normal)]
    class MultiplayerSessionManager_PlayerStateChanged
    {
        static void Postfix(IConnectedPlayer player)
        {
            if (player.isConnectionOwner)
            {
                UI.GameplaySetupPanel.instance.SetCustomSongs(player.HasState("customsongs"));
                UI.GameplaySetupPanel.instance.SetEnforceMods(player.HasState("enforcemods"));
            }
        }
    }

    [HarmonyPatch(typeof(ConnectedPlayerManager), "ResetLocalState", MethodType.Normal)]
    class ConnectedPlayerManager_ResetLocalState
    {
        static void Postfix(ConnectedPlayerManager __instance)
        {
            __instance.SetLocalPlayerState("modded", true);
            __instance.SetLocalPlayerState("customsongs", Plugin.Config.CustomSongs);
            __instance.SetLocalPlayerState("enforcemods", Plugin.Config.EnforceMods);
            Plugin.Log?.Info("Local player state updated.");
        }
    }
}
