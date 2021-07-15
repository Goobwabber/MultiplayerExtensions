using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using System.Reflection;
using Zenject;

namespace MultiplayerExtensions.UI
{
    // GS = GameplaySetup
    // Has all the stuff in the GameplaySetup tab we add
    public class MultiplayerGSViewController : IInitializable
    {
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly LobbySetupFlowCoordinator lobbySetupFlowCoordinator;
        private readonly MultiplayerSettingsPanelController multiplayerSettingsPanelController;
        public static readonly FieldAccessor<GameplaySetupViewController, MultiplayerSettingsPanelController>.Accessor MultiplayerPanelAccessor = FieldAccessor<GameplaySetupViewController, MultiplayerSettingsPanelController>.GetAccessor("_multiplayerSettingsPanelController");

        public MultiplayerGSViewController(MainFlowCoordinator mainFlowCoordinator, LobbySetupFlowCoordinator lobbySetupFlowCoordinator, GameplaySetupViewController gameplaySetupViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.lobbySetupFlowCoordinator = lobbySetupFlowCoordinator;
            multiplayerSettingsPanelController = MultiplayerPanelAccessor(ref gameplaySetupViewController);
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MultiplayerExtensions.UI.MultiplayerGSView.bsml"), multiplayerSettingsPanelController.gameObject, this);
        }

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
    }
}
