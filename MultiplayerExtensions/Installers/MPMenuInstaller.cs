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
            Container.BindInterfacesAndSelfTo<LobbyEnvironmentManager>().AsSingle();
        }

        public override void Start()
        {
            Plugin.Log?.Info("Installing Interface");

            HostLobbySetupViewController hostViewController = Container.Resolve<HostLobbySetupViewController>();
            Container.InstantiateComponent<HostLobbySetupPanel>(hostViewController.gameObject);

            ClientLobbySetupViewController clientViewController = Container.Resolve<ClientLobbySetupViewController>();
            Container.InstantiateComponent<ClientLobbySetupPanel>(clientViewController.gameObject);

            CenterStageScreenController centerScreenController = Container.Resolve<CenterStageScreenController>();
            Container.InstantiateComponent<CenterScreenLoadingPanel>(centerScreenController.gameObject);

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
