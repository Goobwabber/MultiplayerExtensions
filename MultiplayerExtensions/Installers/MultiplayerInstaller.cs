using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Sessions;
using Zenject;

namespace MultiplayerExtensions.Installers
{
	class MultiplayerInstaller : MonoInstaller
	{
		public HarmonyPatchInfo? lobbyPlayerDataPatch;
		public HarmonyPatchInfo? levelLoaderPatch;

		public override void InstallBindings()
		{
			Plugin.Log?.Info("Injecting Dependencies");
			Container.BindInterfacesAndSelfTo<PacketManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<SessionManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<ExtendedPlayerManager>().AsSingle();
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
