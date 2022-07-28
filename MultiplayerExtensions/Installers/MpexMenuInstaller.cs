using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Objects;
using MultiplayerExtensions.Patchers;
using MultiplayerExtensions.UI;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            //Container.BindInterfacesAndSelfTo<MpexPlayerTableCell>().AsSingle();
            Container.BindInterfacesAndSelfTo<AvatarPlacePatcher>().AsSingle();
            Container.BindInterfacesAndSelfTo<MenuEnvironmentPatcher>().AsSingle();

            Container.BindInterfacesAndSelfTo<MpexSetupFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<MpexSettingsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MpexEnvironmentViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MpexMiscViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MpexGameplaySetup>().AsSingle();

            // needed for local player's player place
            var avatarPlace = Container.Resolve<MenuEnvironmentManager>().transform.Find("MultiplayerLobbyEnvironment").Find("LobbyAvatarPlace").gameObject;
            GameObject.Destroy(avatarPlace.GetComponent<MpexAvatarPlaceLighting>());
            Container.Inject(avatarPlace.AddComponent<MpexAvatarPlaceLighting>());
        }
    }
}
