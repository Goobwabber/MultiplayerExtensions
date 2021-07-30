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
        private OpenSettingsViewController openSettingsViewController = null!;
        private SettingsViewController settingsViewController = null!;

        private bool openedSettings;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, PersonalSetupViewController personalSetupViewController,
            MPLobbySetupViewController lobbySetupViewController, OpenSettingsViewController openSettingsViewController, SettingsViewController settingsViewController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            this.personalSetupViewController = personalSetupViewController;
            this.lobbySetupViewController = lobbySetupViewController;
            this.openSettingsViewController = openSettingsViewController;
            this.settingsViewController = settingsViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Multiplayer Preferences");
                showBackButton = true;
                openedSettings = false;
            }
            if (addedToHierarchy)
            {
                ViewController settings = openSettingsViewController;
                if (openedSettings)
                {
                    settings = settingsViewController;
                }
                ProvideInitialViewControllers(personalSetupViewController, lobbySetupViewController, settings);
            }
        }

        protected void Start()
        {
            openSettingsViewController.OpenSettingsButtonClicked += OpenSettingsViewController_OpenSettingsButtonClicked;
        }

        protected void OnDestroy()
        {
            openSettingsViewController.OpenSettingsButtonClicked -= OpenSettingsViewController_OpenSettingsButtonClicked;
        }

        private void OpenSettingsViewController_OpenSettingsButtonClicked()
        {
            openedSettings = true;
            SetRightScreenViewController(settingsViewController, ViewController.AnimationType.In);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
