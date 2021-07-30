using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace MultiplayerExtensions.UI
{
    public class MPSetupFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator parentFlowCoordinator = null!;
        private PersonalSetupViewController personalSetupViewController = null!;
        private MPLobbySetupViewController lobbySetupViewController = null!;
        private SettingsViewController settingsViewController = null!;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, PersonalSetupViewController personalSetupViewController, MPLobbySetupViewController lobbySetupViewController, SettingsViewController settingsViewController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            this.personalSetupViewController = personalSetupViewController;
            this.lobbySetupViewController = lobbySetupViewController;
            this.settingsViewController = settingsViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Multiplayer Preferences");
            showBackButton = true;

            ProvideInitialViewControllers(personalSetupViewController, lobbySetupViewController, settingsViewController);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
