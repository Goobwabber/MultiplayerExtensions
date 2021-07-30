using IPA.Utilities;
using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Extensions;
using MultiplayerExtensions.UI;
using SiraUtil;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MPMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LobbyEnvironmentManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyAvatarManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EmotePanel>().AsSingle();
            Container.BindInterfacesTo<MultiplayerGSViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbySetupFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<MPLobbySetupViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PersonalSetupViewController>().FromNewComponentAsViewController().AsSingle();
        }

        public override void Start()
        {
            Plugin.Log?.Info("Installing Interface");

            CenterStageScreenController centerScreenController = Container.Resolve<CenterStageScreenController>();
            Container.InstantiateComponent<CenterScreenLoadingPanel>(centerScreenController.gameObject);

            ServerPlayerListViewController playerListController = Container.Resolve<ServerPlayerListViewController>();
            GameServerPlayersTableView playersTableView = playerListController.GetField<GameServerPlayersTableView, ServerPlayerListViewController>("_gameServerPlayersTableView");
            GameServerPlayerTableCell playerTableCell = playersTableView.GetField<GameServerPlayerTableCell, GameServerPlayersTableView>("_gameServerPlayerCellPrefab");
            GameServerPlayerTableCell newPlayerTableCell = GameObject.Instantiate(playerTableCell);
            newPlayerTableCell.gameObject.SetActive(false);
            ExtendedPlayerTableCell playerTableCellStub = newPlayerTableCell.gameObject.AddComponent<ExtendedPlayerTableCell>();
            playerTableCellStub.Construct(newPlayerTableCell);
            Destroy(newPlayerTableCell.GetComponent<GameServerPlayerTableCell>());
            playersTableView.SetField<GameServerPlayersTableView, GameServerPlayerTableCell>("_gameServerPlayerCellPrefab", playerTableCellStub);
        }
    }
}
