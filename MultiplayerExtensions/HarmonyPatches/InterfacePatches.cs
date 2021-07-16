using HarmonyLib;
using HMUI;
using IPA.Utilities;
using System;
using UnityEngine.UI;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplaySetupViewController), "Setup", MethodType.Normal)]
    internal class SetupPatch
    {
        public static event Action GameplaySetupChange;
        static void Prefix()
        {
            GameplaySetupChange?.Invoke();
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "DidActivate", MethodType.Normal)]
    internal class DidActivatePatch
    {
        static void Postfix(GameplaySetupViewController ____gameplaySetupViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {
            ____gameplaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.MultiplayerPrivate);
        }
    }

    [HarmonyPatch(typeof(EditableModifiersSelectionView), "interactable", MethodType.Setter)]
    internal class SetInteractablePatch
    {
        // We do not want the modifiers button to be interactable since select beatmap takes us there anyway
        static void Postfix(Button ____editButton)
        {
            ____editButton.interactable = false;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleLobbySetupViewControllerSelectBeatmap", MethodType.Normal)]
    internal class SelectBeatmapPatch
    {
        static void Prefix(SelectModifiersViewController ___selectModifiersViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {
            GameplayModifiers playerGameplayModifiers = ___lobbyPlayersDataModel.GetPlayerGameplayModifiers(___lobbyPlayersDataModel.localUserId);
            ___selectModifiersViewController.Setup(playerGameplayModifiers);
        }
    }

    [HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "initialLeftScreenViewController", MethodType.Getter)]
    internal class SetLeftSelectionViewPatch
    {
        public static event Action EnteredLevelSelection;
        static bool Prefix(ref ViewController __result, LevelSelectionFlowCoordinator __instance, FlowCoordinator ____parentFlowCoordinator)
        {
            if (__instance is MultiplayerLevelSelectionFlowCoordinator && ____parentFlowCoordinator is GameServerLobbyFlowCoordinator gameServerLobbyFlowCoordinator)
            {
                GameplaySetupViewController gameplaySetupViewController = gameServerLobbyFlowCoordinator.GetField<GameplaySetupViewController, GameServerLobbyFlowCoordinator>("_gameplaySetupViewController");
                __result = gameplaySetupViewController;
                EnteredLevelSelection?.Invoke();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleMultiplayerLevelSelectionFlowCoordinatorDidSelectLevel", MethodType.Normal)]
    internal class DidSelectLevelPatch
    {
        static void Prefix(GameplaySetupViewController ____gameplaySetupViewController, SelectModifiersViewController ___selectModifiersViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {

            ___lobbyPlayersDataModel.SetLocalPlayerGameplayModifiers(___selectModifiersViewController.gameplayModifiers);
            ____gameplaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.MultiplayerPrivate);
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "HandleMultiplayerLevelSelectionFlowCoordinatorCancelSelectLevel", MethodType.Normal)]
    internal class CancelSelectLevelPatch
    {
        static void Prefix(GameplaySetupViewController ____gameplaySetupViewController, SelectModifiersViewController ___selectModifiersViewController, ILobbyPlayersDataModel ___lobbyPlayersDataModel)
        {
            ___lobbyPlayersDataModel.SetLocalPlayerGameplayModifiers(___selectModifiersViewController.gameplayModifiers);
            ____gameplaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.MultiplayerPrivate);
        }
    }
}
