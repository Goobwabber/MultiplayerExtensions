using MultiplayerExtensions.Patchers;
using MultiplayerExtensions.Players;
using MultiplayerExtensions.Utilities;
using Zenject;

namespace MultiplayerExtensions.Installers
{
	class MpexAppInstaller : Installer
	{
		private readonly Config _config;

		public MpexAppInstaller(
			Config config)
        {
			_config = config;
        }

        public override void InstallBindings()
        {
			Container.BindInstance(_config).AsSingle();
			Container.BindInterfacesAndSelfTo<SpriteManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MpexPlayerManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<LobbyInstallerPatcher>().AsSingle();
		}
	}
}
