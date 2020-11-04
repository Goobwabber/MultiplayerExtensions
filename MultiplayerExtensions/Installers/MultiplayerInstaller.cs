using CustomAvatar.Avatar;
using MultiplayerExtensions.Avatars;
using MultiplayerExtensions.Downloaders;
using MultiplayerExtensions.Networking;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MultiplayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.Log?.Info("Injecting Dependencies");
            Container.BindInterfacesAndSelfTo<ExtendedSessionManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ExtendedPlayerManager>().AsSingle();

            if (IPA.Loader.PluginManager.GetPluginFromId("CustomAvatar") != null)
            {
                BindCustomAvatars(Container);
            }
        }

        private void BindCustomAvatars(DiContainer Container)
        {
            Plugin.Log?.Info("Found CustomAvatar");
            Container.Bind<IAvatarProvider<LoadedAvatar>>()
                .To<ModelSaber>()
                .AsSingle();
            Container.Bind(typeof(IInitializable), typeof(CustomAvatarManager)).To<CustomAvatarManager>().AsSingle();
            Container.QueueForInject(typeof(ModelSaber));
            Container.Resolve<MultiplayerLobbyAvatarController>().gameObject.AddComponent<CustomLobbyAvatarController>();
        }
    }
}
