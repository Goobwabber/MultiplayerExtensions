using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MultiplayerInstaller : MonoInstaller
    {
        public HarmonyPatchInfo? lobbyPlayerDataPatch;

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

            if (lobbyPlayerDataPatch != null)
                HarmonyManager.ApplyPatch(lobbyPlayerDataPatch);
        }

        
    }
}
