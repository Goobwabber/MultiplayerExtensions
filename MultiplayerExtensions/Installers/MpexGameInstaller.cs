using IPA.Utilities;
using MultiplayerExtensions.Environment;
using MultiplayerExtensions.Patchers;
using MultiplayerExtensions.Players;
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
            Container.BindInterfacesAndSelfTo<PlayerPositionPatcher>().AsSingle();
            Container.BindInterfacesAndSelfTo<ColorSchemePatcher>().AsSingle();
            Container.RegisterRedecorator(new LocalActivePlayerRegistration(DecorateLocalActivePlayerFacade));
            Container.RegisterRedecorator(new LocalActivePlayerDuelRegistration(DecorateLocalActivePlayerFacade));
            Container.RegisterRedecorator(new ConnectedPlayerRegistration(DecorateConnectedPlayerFacade));
            Container.RegisterRedecorator(new ConnectedPlayerDuelRegistration(DecorateConnectedPlayerFacade));
        }

        private MultiplayerLocalActivePlayerFacade DecorateLocalActivePlayerFacade(MultiplayerLocalActivePlayerFacade original)
        {
            if (Plugin.Config.MissLighting)
            original.gameObject.AddComponent<MpexPlayerFacadeLighting>();
            return original;
        }

        private MultiplayerConnectedPlayerFacade DecorateConnectedPlayerFacade(MultiplayerConnectedPlayerFacade original)
        {
            if (Plugin.Config.MissLighting && !Plugin.Config.PersonalMissLightingOnly)
                original.gameObject.AddComponent<MpexPlayerFacadeLighting>();
            original.gameObject.AddComponent<MpexConnectedObjectManager>();
            return original;
        }
    }
}
