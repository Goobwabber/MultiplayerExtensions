using HarmonyLib;
using HMUI;
using IPA.Utilities;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleLobbySetupViewControllerSelectBeatmap", MethodType.Normal)]
    internal class SelectBeatmapPatch
    {
        static void Prefix(GameServerLobbyFlowCoordinator __instance, MultiplayerLevelSelectionFlowCoordinator ___multiplayerLevelSelectionFlowCoordinator, SelectModifiersViewController ___selectModifiersViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {
            GameplayModifiers playerGameplayModifiers = ___lobbyPlayersDataModel.GetPlayerGameplayModifiers(___lobbyPlayersDataModel.localUserId);
            ___selectModifiersViewController.Setup(playerGameplayModifiers);
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleLobbySetupViewControllerSelectModifiers", MethodType.Normal)]
    internal class SelectModifiersPatch
    {
        static bool Prefix(GameServerLobbyFlowCoordinator __instance)
        {
            __instance.InvokeMethod<object, GameServerLobbyFlowCoordinator>("HandleLobbySetupViewControllerSelectBeatmap");
            return false;
        }
    }

    [HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "initialLeftScreenViewController", MethodType.Getter)]
    internal class SetLeftSelectionViewPatch
    {
        static bool Prefix(ref ViewController __result, LevelSelectionFlowCoordinator __instance, FlowCoordinator ____parentFlowCoordinator)
        {
            if (__instance is MultiplayerLevelSelectionFlowCoordinator && ____parentFlowCoordinator is GameServerLobbyFlowCoordinator gameServerLobbyFlowCoordinator)
            {
                SelectModifiersViewController selectModifiersViewController = gameServerLobbyFlowCoordinator.GetField<SelectModifiersViewController, GameServerLobbyFlowCoordinator>("selectModifiersViewController");
                __result = selectModifiersViewController;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleMultiplayerLevelSelectionFlowCoordinatorDidSelectLevel", MethodType.Normal)]
    internal class DidSelectLevelPatch
    {
        static void Prefix(SelectModifiersViewController ___selectModifiersViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {
            ___lobbyPlayersDataModel.SetLocalPlayerGameplayModifiers(___selectModifiersViewController.gameplayModifiers);
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleMultiplayerLevelSelectionFlowCoordinatorCancelSelectLevel", MethodType.Normal)]
    internal class CancelSelectLevelPatch
    {
        static void Prefix(SelectModifiersViewController ___selectModifiersViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {
            ___lobbyPlayersDataModel.SetLocalPlayerGameplayModifiers(___selectModifiersViewController.gameplayModifiers);
        }
    }
}
