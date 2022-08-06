using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using SiraUtil.Affinity;
using System;

namespace MultiplayerExtensions.UI
{
    public class MpexSetupFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator parentFlowCoordinator = null!;
        private MpexSettingsViewController _settingsViewController = null!;
        private MpexEnvironmentViewController _environmentViewController = null!;
        private MpexMiscViewController _miscViewController = null!;
        private ILobbyGameStateController _gameStateController = null!;

        [Inject]
        public void Construct(
            MainFlowCoordinator mainFlowCoordinator, 
            MpexSettingsViewController settingsViewController,
            MpexEnvironmentViewController environmentViewController,
            MpexMiscViewController miscViewController,
            ILobbyGameStateController gameStateController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            _settingsViewController = settingsViewController;
            _environmentViewController = environmentViewController;
            _miscViewController = miscViewController;
            _gameStateController = gameStateController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Multiplayer Preferences");
                showBackButton = true;
            }
            if (addedToHierarchy)
            {
                ProvideInitialViewControllers(_settingsViewController, _environmentViewController, _miscViewController);
                _gameStateController.gameStartedEvent += DismissGameStartedEvent;
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            if (removedFromHierarchy)
                _gameStateController.gameStartedEvent -= DismissGameStartedEvent;
        }

        private void DismissGameStartedEvent(ILevelGameplaySetupData obj)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal, true);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
