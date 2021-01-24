using LiteNetLib.Utils;
using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Sessions
{
    public class SessionManager : IInitializable, IDisposable
    {
        protected readonly IMultiplayerSessionManager _sessionManager;

        internal SessionManager(IMultiplayerSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public void Initialize()
        {
            Plugin.Log?.Info("Setting up SessionManager");

            MPState.CustomSongsEnabled = Plugin.Config.CustomSongs;

            _sessionManager.SetLocalPlayerState("modded", true);
            _sessionManager.SetLocalPlayerState("customsongs", Plugin.Config.CustomSongs);
            _sessionManager.SetLocalPlayerState("enforcemods", Plugin.Config.EnforceMods);
            _sessionManager.playerStateChangedEvent += HandlePlayerStateChanged;
        }

        public void Dispose()
        {
            _sessionManager.playerStateChangedEvent -= HandlePlayerStateChanged;
        }

        private void HandlePlayerStateChanged(IConnectedPlayer player)
        {
            if (player.isConnectionOwner)
                MPState.CustomSongsEnabled = player.HasState("customsongs");
        }
    }
}
