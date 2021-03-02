using System;
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
            MPState.FreeModEnabled = Plugin.Config.FreeMod;

            _sessionManager.SetLocalPlayerState("modded", true);
            _sessionManager.SetLocalPlayerState("customsongs", Plugin.Config.CustomSongs);
            _sessionManager.SetLocalPlayerState("freemod", Plugin.Config.FreeMod);
            _sessionManager.connectedEvent += HandleConnected;
            _sessionManager.playerStateChangedEvent += HandlePlayerStateChanged;
        }

        public void Dispose()
        {
            _sessionManager.playerStateChangedEvent -= HandlePlayerStateChanged;
        }

        private void HandleConnected()
        {
            MPState.LocalPlayerIsHost = _sessionManager.localPlayer.isConnectionOwner;
        }

        private void HandlePlayerStateChanged(IConnectedPlayer player)
        {
            if (player.isConnectionOwner)
            {
                if (MPState.CustomSongsEnabled != player.HasState("customsongs"))
                {
                    MPState.CustomSongsEnabled = player.HasState("customsongs");
                    MPEvents.RaiseCustomSongsChanged(this, player.HasState("customsongs"));
                }
                
                if (MPState.FreeModEnabled != player.HasState("freemod"))
                {
                    MPState.FreeModEnabled = player.HasState("freemod");
                    MPEvents.RaiseCustomSongsChanged(this, player.HasState("freemod"));
                }
            }
        }
    }
}
