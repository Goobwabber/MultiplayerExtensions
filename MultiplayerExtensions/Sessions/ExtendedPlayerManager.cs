using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zenject;

namespace MultiplayerExtensions.Sessions
{
    class ExtendedPlayerManager : IInitializable
    {
        [Inject]
        private SessionManager _sessionManager;

        [Inject]
        private PacketManager _packetManager;

        private Dictionary<string, ExtendedPlayer> _players = new Dictionary<string, ExtendedPlayer>();
        public string localPlatformID;

        public void Initialize()
        {
            Plugin.Log?.Info("Setting up PlayerManager");

            _sessionManager.playerConnectedEvent += OnPlayerConnected;
            _sessionManager.playerDisconnectedEvent += OnPlayerDisconnected;

            _packetManager.RegisterCallback<ExtendedPlayerPacket>(HandlePlayerPacket);

            BS_Utils.Gameplay.GetUserInfo.GetUserAsync().ContinueWith(r =>
            {
                localPlatformID = r.Result.platformUserId;
            });
        }

        private void OnPlayerConnected(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"Player '{player.userId}' joined");
            var extendedPlayer = new ExtendedPlayer(player);
            _players[player.userId] = extendedPlayer;
            
            if (localPlatformID != null)
            {
                ExtendedPlayerPacket localPlayerPacket = new ExtendedPlayerPacket().Init(localPlatformID);
                _packetManager.Send(localPlayerPacket);
            }
        }

        private void OnPlayerDisconnected(IConnectedPlayer player)
        {
            Plugin.Log?.Info($"Player '{player.userId}' disconnected");
            var extendedPlayer = _players[player.userId];
            _players.Remove(player.userId);
        }

        private void HandlePlayerPacket(ExtendedPlayerPacket packet, IConnectedPlayer player)
        {
            Plugin.Log?.Info($"Received 'ExtendedPlayerPacket' from '{player.userId}' with platformID: '{packet.platformID}'  mpexVersion: '{packet.mpexVersion}'");
            var extendedPlayer = _players[player.userId];
            extendedPlayer.platformID = packet.platformID;
            extendedPlayer.mpexVersion = new SemVer.Version(packet.mpexVersion);
            if (Plugin.PluginMetadata.Version != extendedPlayer.mpexVersion) 
            {
                Plugin.Log?.Warn("###################################################################");
                Plugin.Log?.Warn("Different MultiplayerExtensions version detected!");
                Plugin.Log?.Warn($"The player '{player.userName}' is using MultiplayerExtensions {extendedPlayer.mpexVersion} while you are using MultiplayerExtensions {Plugin.PluginMetadata.Version}");
                Plugin.Log?.Warn("For best compatibility all players should use the same version of MultiplayerExtensions.");
                Plugin.Log?.Warn("###################################################################");
            }
        }

        public ExtendedPlayer GetExtendedPlayer(IConnectedPlayer player)
        {
            return _players[player.userId];
        }
    }
}
