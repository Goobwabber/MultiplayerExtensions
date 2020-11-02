using MultiplayerExtensions.Controllers;
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
                Plugin.Log?.Info("Found CustomAvatar");
                Container.Bind(typeof(IInitializable), typeof(AvatarController)).To<AvatarController>().AsSingle();
            }
        }
    }
}
