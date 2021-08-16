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
        public static event Action? GameplaySetupChange;
        static void Prefix()
        {
            GameplaySetupChange?.Invoke();
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "DidActivate", MethodType.Normal)]
    internal class DidActivatePatch
    {
        static void Postfix(GameplaySetupViewController ____gameplaySetupViewController)
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

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleLobbySetupViewControllerSelectBeatmap), MethodType.Normal)]
    internal class SelectBeatmapPatch
    {
        static void Prefix(SelectModifiersViewController ____selectModifiersViewController, ILobbyPlayersDataModel ____lobbyPlayersDataModel)
        {
            GameplayModifiers playerGameplayModifiers = ____lobbyPlayersDataModel.GetPlayerGameplayModifiers(____lobbyPlayersDataModel.localUserId);
            ____selectModifiersViewController.Setup(playerGameplayModifiers);
            if (playerGameplayModifiers == null)
            {
                Plugin.Log.Debug("Bruh");
            }
        }
    }

    [HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "initialLeftScreenViewController", MethodType.Getter)]
    internal class SetLeftSelectionViewPatch
    {
        public static event Action? EnteredLevelSelection;
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

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleMultiplayerLevelSelectionFlowCoordinatorDidSelectLevel), MethodType.Normal)]
    internal class DidSelectLevelPatch
    {
        static void Prefix(GameplaySetupViewController ____gameplaySetupViewController, SelectModifiersViewController ____selectModifiersViewController, ILobbyPlayersDataModel ____lobbyPlayersDataModel)
        {

            ____lobbyPlayersDataModel.SetLocalPlayerGameplayModifiers(____selectModifiersViewController.gameplayModifiers);
            ____gameplaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.MultiplayerPrivate);
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleMultiplayerLevelSelectionFlowCoordinatorCancelSelectLevel), MethodType.Normal)]
    internal class CancelSelectLevelPatch
    {
        static void Prefix(GameplaySetupViewController ____gameplaySetupViewController, SelectModifiersViewController ____selectModifiersViewController, ILobbyPlayersDataModel ____lobbyPlayersDataModel)
        {
            ____lobbyPlayersDataModel.SetLocalPlayerGameplayModifiers(____selectModifiersViewController.gameplayModifiers);
            ____gameplaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.MultiplayerPrivate);
        }
    }
}
