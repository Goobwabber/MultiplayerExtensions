using IPA.Utilities;
using MultiplayerExtensions.Objects;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexMenuInstaller : MonoInstaller
    {
        private readonly FieldAccessor<ServerPlayerListViewController, GameServerPlayersTableView>.Accessor _gameServerPlayersTableView
            = FieldAccessor<ServerPlayerListViewController, GameServerPlayersTableView>
                .GetAccessor(nameof(_gameServerPlayersTableView));

        private readonly FieldAccessor<GameServerPlayersTableView, GameServerPlayerTableCell>.Accessor _gameServerPlayerCellPrefab
            = FieldAccessor<GameServerPlayersTableView, GameServerPlayerTableCell>
                .GetAccessor(nameof(_gameServerPlayerCellPrefab));

        public override void InstallBindings()
        {
            
        }

        public override void Start()
        {
            var playerListController = Container.Resolve<ServerPlayerListViewController>();
            var playersTableView = _gameServerPlayersTableView(ref playerListController);
            RedecoratePlayerTableCell(ref _gameServerPlayerCellPrefab(ref playersTableView));
        }

        private void RedecoratePlayerTableCell(ref GameServerPlayerTableCell originalPrefab)
        {
            if (originalPrefab.transform.parent != null && originalPrefab.transform.parent.name == "MultiplayerDecorator")
                return;

            GameObject mdgo = new("MultiplayerDecorator");
            mdgo.SetActive(false);
            var prefab = Object.Instantiate(originalPrefab, mdgo.transform);

            prefab.gameObject.SetActive(false);
            var playerTableCell = prefab.gameObject.AddComponent<MpexPlayerTableCell>();
            playerTableCell.Construct(originalPrefab);
            GameObject.Destroy(prefab.GetComponent<GameServerPlayerTableCell>());

            originalPrefab = prefab;
        }
    }
}
