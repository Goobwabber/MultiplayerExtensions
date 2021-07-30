﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using MultiplayerExtensions.HarmonyPatches;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zenject;

namespace MultiplayerExtensions.UI
{
    // GS = GameplaySetup
    // Has all the stuff in the GameplaySetup tab we add
    public class MultiplayerGSViewController : IInitializable, IDisposable
    {
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly LobbySetupFlowCoordinator lobbySetupFlowCoordinator;
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private readonly EmotePanel emotePanel;
        private readonly TextSegmentedControl selectionSegmentedControl;
        private readonly GameplayModifiersPanelController singleplayerModifiersPanelController;
        private readonly GameplayModifiersPanelController multiplayerModifiersPanelController;
        private readonly MultiplayerSettingsPanelController multiplayerSettingsPanelController;

        public MultiplayerGSViewController(MainFlowCoordinator mainFlowCoordinator, LobbySetupFlowCoordinator lobbySetupFlowCoordinator, GameplaySetupViewController gameplaySetupViewController,
            SelectModifiersViewController selectModifiersViewController, EmotePanel emotePanel)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.lobbySetupFlowCoordinator = lobbySetupFlowCoordinator;
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.emotePanel = emotePanel;

            selectionSegmentedControl = gameplaySetupViewController.GetField<TextSegmentedControl, GameplaySetupViewController>("_selectionSegmentedControl");
            singleplayerModifiersPanelController = gameplaySetupViewController.GetField<GameplayModifiersPanelController, GameplaySetupViewController>("_gameplayModifiersPanelController");
            multiplayerModifiersPanelController = selectModifiersViewController.GetField<GameplayModifiersPanelController, SelectModifiersViewController>("_gameplayModifiersPanelController");
            multiplayerSettingsPanelController = gameplaySetupViewController.GetField<MultiplayerSettingsPanelController, GameplaySetupViewController>("_multiplayerSettingsPanelController");
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MultiplayerExtensions.UI.MultiplayerGSView.bsml"), multiplayerSettingsPanelController.gameObject, this);
            multiplayerModifiersPanelController.transform.SetParent(singleplayerModifiersPanelController.transform.parent);
            multiplayerModifiersPanelController.transform.localPosition = singleplayerModifiersPanelController.transform.localPosition;
            multiplayerModifiersPanelController.gameObject.SetActive(false);
            multiplayerModifiersPanelController.gameObject.name = "MultiplayerModifiersPanelController";

            SetLeftSelectionViewPatch.EnteredLevelSelection += ShowMultiplayerModifiersPanel;
            SetupPatch.GameplaySetupChange += HideMultiplayerModifiersPanel;
        }

        public void Dispose()
        {
            SetLeftSelectionViewPatch.EnteredLevelSelection -= ShowMultiplayerModifiersPanel;
            SetupPatch.GameplaySetupChange -= HideMultiplayerModifiersPanel;
        }

        private void ShowMultiplayerModifiersPanel()
        {
            List<GameplaySetupViewController.Panel> panels = gameplaySetupViewController.GetField<List<GameplaySetupViewController.Panel>, GameplaySetupViewController>("_panels");
            panels[0].gameObject.SetActive(false);
            panels.RemoveAt(0);
            panels.Insert(0, new GameplaySetupViewController.Panel(Localization.Get("BUTTON_MODIFIERS"), multiplayerModifiersPanelController, multiplayerModifiersPanelController.gameObject));
            List<string> panelTitles = new List<string>(panels.Count);
            foreach (GameplaySetupViewController.Panel panel in panels)
            {
                panelTitles.Add(panel.title);
            }
            selectionSegmentedControl.SetTexts(panelTitles);
            gameplaySetupViewController.SetActivePanel(0);
        }

        private void HideMultiplayerModifiersPanel() => multiplayerModifiersPanelController.gameObject.SetActive(false);

        [UIAction("lobby-settings-click")]
        private void PresentLobbySettings()
        {
            FlowCoordinator deepestChildFlowCoordinator = DeepestChildFlowCoordinator(mainFlowCoordinator);
            lobbySetupFlowCoordinator.parentFlowCoordinator = deepestChildFlowCoordinator;
            deepestChildFlowCoordinator.PresentFlowCoordinator(lobbySetupFlowCoordinator);
        }

        private FlowCoordinator DeepestChildFlowCoordinator(FlowCoordinator root)
        {
            var flow = root.childFlowCoordinator;
            if (flow == null) return root;
            if (flow.childFlowCoordinator == null || flow.childFlowCoordinator == flow)
            {
                return flow;
            }
            return DeepestChildFlowCoordinator(flow);
        }

        [UIAction("spawn-emote-panel")]
        private void SpawnEmotePanel()
        {
            emotePanel.ToggleActive();
        }
    }
}