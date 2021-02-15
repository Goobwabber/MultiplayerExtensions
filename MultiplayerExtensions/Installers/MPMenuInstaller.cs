using IPA.Utilities;
using MultiplayerExtensions.Environments;
using MultiplayerExtensions.OverrideClasses;
using MultiplayerExtensions.UI;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MPMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LobbyPlaceManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyEnvironmentManager>().AsSingle();
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
            GameServerPlayerTableCell newPlayerTableCell = GameObject.Instantiate(playerTableCell);
            newPlayerTableCell.gameObject.SetActive(false);
            PlayerTableCellStub playerTableCellStub = newPlayerTableCell.gameObject.AddComponent<PlayerTableCellStub>();
            playerTableCellStub.Construct(newPlayerTableCell);
            Destroy(newPlayerTableCell.GetComponent<GameServerPlayerTableCell>());
            playersTableView.SetField<GameServerPlayersTableView, GameServerPlayerTableCell>("_gameServerPlayerCellPrefab", playerTableCellStub);
        }
    }
}
