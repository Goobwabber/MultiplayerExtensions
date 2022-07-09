using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace MultiplayerExtensions.UI
{
    public class MpexSetupFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator parentFlowCoordinator = null!;
        private MpexSettingsViewController _settingsViewController = null!;
        private EmptyViewController _emptyViewController = null!;

        [Inject]
        public void Construct(
            MainFlowCoordinator mainFlowCoordinator, 
            MpexSettingsViewController settingsViewController,
            EmptyViewController emptyViewController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            _settingsViewController = settingsViewController;
            _emptyViewController = emptyViewController;
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
                ProvideInitialViewControllers(_emptyViewController, _settingsViewController);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
