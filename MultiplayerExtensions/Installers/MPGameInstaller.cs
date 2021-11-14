using IPA.Utilities;
using MultiplayerExtensions.Environments;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MPGameInstaller : Installer
    {
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
