using IPA.Utilities;
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
            
        }
    }
}
