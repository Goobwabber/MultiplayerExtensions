using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Objects;
using MultiplayerExtensions.Patchers;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MpexPlayerTableCell>().AsSingle();
            Container.BindInterfacesAndSelfTo<AvatarPlacePatcher>().AsSingle();
            var avatarPlace = Container.Resolve<MenuEnvironmentManager>().transform.Find("MultiplayerLobbyEnvironment").Find("LobbyAvatarPlace").gameObject;
            GameObject.Destroy(avatarPlace.GetComponent<MpexAvatarPlaceLighting>());
            Container.Inject(avatarPlace.AddComponent<MpexAvatarPlaceLighting>());
        }
    }
}
