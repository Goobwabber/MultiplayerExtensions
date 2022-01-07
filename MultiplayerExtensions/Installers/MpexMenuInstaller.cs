using IPA.Utilities;
using MultiplayerExtensions.Objects;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            
        }

        public override void Start()
        {
            ServerPlayerListViewController playerListController = Container.Resolve<ServerPlayerListViewController>();
            GameServerPlayersTableView playersTableView = playerListController.GetField<GameServerPlayersTableView, ServerPlayerListViewController>("_gameServerPlayersTableView");
            GameServerPlayerTableCell playerTableCell = playersTableView.GetField<GameServerPlayerTableCell, GameServerPlayersTableView>("_gameServerPlayerCellPrefab");
            GameServerPlayerTableCell newPlayerTableCell = GameObject.Instantiate(playerTableCell);
            newPlayerTableCell.gameObject.SetActive(false);
            MpexPlayerTableCell playerTableCellStub = newPlayerTableCell.gameObject.AddComponent<MpexPlayerTableCell>();
            playerTableCellStub.Construct(newPlayerTableCell);
            Destroy(newPlayerTableCell.GetComponent<GameServerPlayerTableCell>());
            playersTableView.SetField<GameServerPlayersTableView, GameServerPlayerTableCell>("_gameServerPlayerCellPrefab", playerTableCellStub);
        }
    }
}
