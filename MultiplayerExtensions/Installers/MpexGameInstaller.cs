using IPA.Utilities;
using MultiplayerExtensions.Environment;
using SiraUtil.Extras;
using SiraUtil.Objects.Multiplayer;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.RegisterRedecorator(new LocalActivePlayerRegistration(DecorateLocalActivePlayerFacade));
            Container.RegisterRedecorator(new LocalActivePlayerDuelRegistration(DecorateLocalActivePlayerFacade));
            Container.RegisterRedecorator(new ConnectedPlayerRegistration(DecorateConnectedPlayerFacade));
            Container.RegisterRedecorator(new ConnectedPlayerDuelRegistration(DecorateConnectedPlayerFacade));
        }

        private MultiplayerLocalActivePlayerFacade DecorateLocalActivePlayerFacade(MultiplayerLocalActivePlayerFacade original)
        {
            original.gameObject.AddComponent<MpexPlayerFacadeLighting>();
            return original;
        }

        private MultiplayerConnectedPlayerFacade DecorateConnectedPlayerFacade(MultiplayerConnectedPlayerFacade original)
        {
            original.gameObject.AddComponent<MpexPlayerFacadeLighting>();
            return original;
        }
    }
}
