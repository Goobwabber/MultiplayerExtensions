using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using SiraUtil.Affinity;

namespace MultiplayerExtensions.UI
{
    public class MpexSetupFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator parentFlowCoordinator = null!;
        private MpexSettingsViewController _settingsViewController = null!;
        private MpexEnvironmentViewController _environmentViewController = null!;
        private MpexMiscViewController _miscViewController = null!;

        [Inject]
        public void Construct(
            MainFlowCoordinator mainFlowCoordinator, 
            MpexSettingsViewController settingsViewController,
            MpexEnvironmentViewController environmentViewController,
            MpexMiscViewController miscViewController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            _settingsViewController = settingsViewController;
            _environmentViewController = environmentViewController;
            _miscViewController = miscViewController;
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
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
