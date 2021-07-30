using MultiplayerExtensions.Extensions;
using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Utilities;
using Zenject;

namespace MultiplayerExtensions.Installers
{
	class MPCoreInstaller : MonoInstaller
	{
		public HarmonyPatchInfo? lobbyPlayerDataPatch;
		public HarmonyPatchInfo? levelLoaderPatch;

        public override void InstallBindings()
        {
            Plugin.Log?.Info("Injecting Dependencies");

            Container.BindInterfacesAndSelfTo<PacketManager>().AsSingle();
			var _ = Container.Resolve<NetworkConfigSO>().masterServerEndPoint;

			Container.Inject(Container.Resolve<IMultiplayerSessionManager>() as ExtendedSessionManager);
		}

		public override void Start()
		{
			lobbyPlayerDataPatch = HarmonyManager.GetPatch<LobbyPlayersDataModelPatch>();
			levelLoaderPatch = HarmonyManager.GetPatch<LevelLoaderPatch>();

			if (lobbyPlayerDataPatch != null)
				HarmonyManager.ApplyPatch(lobbyPlayerDataPatch);
			
			if (levelLoaderPatch != null)
				HarmonyManager.ApplyPatch(levelLoaderPatch);
		}
	}
}
