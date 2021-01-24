using IPA.Utilities;
using MultiplayerExtensions.Environments;
using MultiplayerExtensions.OverrideClasses;
using MultiplayerExtensions.UI;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class InterfaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LobbyPlaceManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerColorManager>().AsSingle();
        }

        public override void Start()
        {
            Plugin.Log?.Info("Installing Interface");

            HostLobbySetupViewController hostViewController = Container.Resolve<HostLobbySetupViewController>();
            HostLobbySetupPanel hostSetupPanel = hostViewController.gameObject.AddComponent<HostLobbySetupPanel>();
            Container.Inject(hostSetupPanel);

            ClientLobbySetupViewController clientViewController = Container.Resolve<ClientLobbySetupViewController>();
            ClientLobbySetupPanel clientSetupPanel = clientViewController.gameObject.AddComponent<ClientLobbySetupPanel>();
            Container.Inject(clientSetupPanel);

            CenterStageScreenController centerScreenController = Container.Resolve<CenterStageScreenController>();
            CenterScreenLoadingPanel loadingPanel = centerScreenController.gameObject.AddComponent<CenterScreenLoadingPanel>();
            Container.Inject(loadingPanel);

            ServerPlayerListController playerListController = Container.Resolve<ServerPlayerListController>();
            GameServerPlayersTableView playersTableView = playerListController.GetField<GameServerPlayersTableView, ServerPlayerListController>("_gameServerPlayersTableView");
            GameServerPlayerTableCell playerTableCell = playersTableView.GetField<GameServerPlayerTableCell, GameServerPlayersTableView>("_gameServerPlayerCellPrefab");
            PlayerTableCellStub playerTableCellStub = playerTableCell.gameObject.AddComponent<PlayerTableCellStub>();
            playerTableCellStub.Construct(playerTableCell);
            Destroy(playerTableCell.GetComponent<GameServerPlayerTableCell>());
            playersTableView.SetField<GameServerPlayersTableView, GameServerPlayerTableCell>("_gameServerPlayerCellPrefab", playerTableCellStub);
        }
    }
}
