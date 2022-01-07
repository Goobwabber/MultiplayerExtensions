using IPA.Utilities;
using MultiplayerExtensions.Environments;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexGameInstaller : Installer
    {
        private readonly FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>.Accessor _activeLocalPlayerControllerPrefab =
            FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>
            .GetAccessor("_activeLocalPlayerControllerPrefab");

        private readonly FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>.Accessor _activeLocalPlayerDuelControllerPrefab =
            FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>
            .GetAccessor("_activeLocalPlayerDuelControllerPrefab");

        public override void InstallBindings()
        {
            MultiplayerPlayersManager playersManager = Container.Resolve<MultiplayerPlayersManager>();
            playersManager.GetField<MultiplayerLocalActivePlayerFacade, MultiplayerPlayersManager>("_activeLocalPlayerControllerPrefab").gameObject.AddComponent<MultiplayerActivePlayer>();
            playersManager.GetField<MultiplayerLocalActivePlayerFacade, MultiplayerPlayersManager>("_activeLocalPlayerDuelControllerPrefab").gameObject.AddComponent<MultiplayerActivePlayer>();
            playersManager.GetField<MultiplayerConnectedPlayerFacade, MultiplayerPlayersManager>("_connectedPlayerControllerPrefab").gameObject.AddComponent<MultiplayerActivePlayer>();
            playersManager.GetField<MultiplayerConnectedPlayerFacade, MultiplayerPlayersManager>("_connectedPlayerDuelControllerPrefab").gameObject.AddComponent<MultiplayerActivePlayer>();
        }
    }
}
