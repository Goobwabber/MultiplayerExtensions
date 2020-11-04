using CustomAvatar.Avatar;
using MultiplayerExtensions.Avatars;
using MultiplayerExtensions.Downloaders;
using Zenject;

namespace MultiplayerExtensions.Zenject
{
    class MultiplayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.Log?.Info("Injecting Dependencies");
            Container.Bind(typeof(IInitializable), typeof(CustomMultiplayerController)).To<CustomMultiplayerController>().AsSingle().NonLazy();

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
            Container.Bind(typeof(IInitializable), typeof(AvatarController)).To<AvatarController>().AsSingle();
            Container.QueueForInject(typeof(ModelSaber));
        }
    }
}
