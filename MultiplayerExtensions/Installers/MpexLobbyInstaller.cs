using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Environments.Lobby;
using SiraUtil.Extras;
using SiraUtil.Objects.Multiplayer;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexLobbyInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.RegisterRedecorator(new LobbyAvatarPlaceRegistration(DecorateAvatarPlace));
            Container.RegisterRedecorator(new LobbyAvatarRegistration(DecorateAvatar));
        }

        private MultiplayerLobbyAvatarPlace DecorateAvatarPlace(MultiplayerLobbyAvatarPlace original)
        {
            original.gameObject.AddComponent<MpexAvatarPlaceLighting>();
            return original;
        }

        private MultiplayerLobbyAvatarController DecorateAvatar(MultiplayerLobbyAvatarController original)
        {
            var avatarCaption = original.transform.Find("AvatarCaption").gameObject;
            avatarCaption.AddComponent<MpexAvatarNameTag>();

            return original;
        }
    }
}
