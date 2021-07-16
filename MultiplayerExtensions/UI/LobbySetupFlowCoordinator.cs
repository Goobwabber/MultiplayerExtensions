using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace MultiplayerExtensions.UI
{
    public class LobbySetupFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator parentFlowCoordinator = null!;
        private PersonalSetupViewController personalSetupViewController = null!;
        private LobbySetupViewController lobbySetupViewController = null!;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, PersonalSetupViewController personalSetupViewController, LobbySetupViewController lobbySetupViewController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            this.personalSetupViewController = personalSetupViewController;
            this.lobbySetupViewController = lobbySetupViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Multiplayer Preferences");
            showBackButton = true;

            ProvideInitialViewControllers(lobbySetupViewController, personalSetupViewController);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
