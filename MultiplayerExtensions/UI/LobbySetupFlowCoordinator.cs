using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace MultiplayerExtensions.UI
{
    public class LobbySetupFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator? parentFlowCoordinator;
        private PersonalSetupViewController? personalSetupViewController;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, PersonalSetupViewController personalSetupViewController)
        {
            parentFlowCoordinator = mainFlowCoordinator;
            this.personalSetupViewController = personalSetupViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Multiplayer Preferences");
            showBackButton = true;

            ProvideInitialViewControllers(personalSetupViewController);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
