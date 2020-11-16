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
    class SessionManager : IInitializable
    {
        [Inject]
        private IMultiplayerSessionManager _sessionManager;

        [Inject]
        private PacketManager _packetManager;

        public PacketSerializer serializer = new PacketSerializer();

        public void Initialize()
        {
            Plugin.Log?.Info("Setting up SessionManager");
            _packetManager.RegisterSerializer(serializer);
            _sessionManager.playerStateChangedEvent += HandlePlayerStateChanged;

            _sessionManager.SetLocalPlayerState("modded", true);
            _sessionManager.SetLocalPlayerState("customsongs", Plugin.Config.CustomSongs);
            _sessionManager.SetLocalPlayerState("enforcemods", Plugin.Config.EnforceMods);

            _sessionManager.connectedEvent += delegate () { connectedEvent?.Invoke(); };
            _sessionManager.connectionFailedEvent += delegate (ConnectionFailedReason reason) { connectionFailedEvent?.Invoke(reason); };
            _sessionManager.playerConnectedEvent += delegate (IConnectedPlayer player) { playerConnectedEvent?.Invoke(player); };
            _sessionManager.playerDisconnectedEvent += delegate (IConnectedPlayer player) { playerDisconnectedEvent?.Invoke(player); };
            _sessionManager.playerStateChangedEvent += delegate (IConnectedPlayer player) { playerStateChangedEvent?.Invoke(player); };
            _sessionManager.disconnectedEvent += delegate (DisconnectedReason reason) { disconnectedEvent?.Invoke(reason); };
        }

        private void HandlePlayerStateChanged(IConnectedPlayer player)
        {
            if (player.userId != _sessionManager.localPlayer.userId)
            {
                if (player.isConnectionOwner)
                {
                    UI.GameplaySetupPanel.instance.SetCustomSongs(player.HasState("customsongs"));
                    UI.GameplaySetupPanel.instance.SetEnforceMods(player.HasState("enforcemods"));
                }
            }
        }

        public event Action connectedEvent;
        public event Action<ConnectionFailedReason> connectionFailedEvent;
        public event Action<IConnectedPlayer> playerConnectedEvent;
        public event Action<IConnectedPlayer> playerDisconnectedEvent;
        public event Action<IConnectedPlayer> playerStateChangedEvent;
        public event Action<DisconnectedReason> disconnectedEvent;

        public IConnectedPlayer localPlayer => _sessionManager.localPlayer;
        public bool isConnectionOwner => _sessionManager.isConnectionOwner;
        public float syncTime => _sessionManager.syncTime;
        public bool isSyncTimeInitialized => _sessionManager.isSyncTimeInitialized;
        public float syncTimeDelay => _sessionManager.syncTimeDelay;
        public int connectedPlayerCount => _sessionManager.connectedPlayerCount;
        public bool isConnectingOrConnected => _sessionManager.isConnectingOrConnected;
        public bool isConnected => _sessionManager.isConnected;
        public bool isConnecting => _sessionManager.isConnecting;
        public bool isSpectating => _sessionManager.isSpectating;
        public IReadOnlyList<IConnectedPlayer> connectedPlayers => _sessionManager.connectedPlayers;
        public IConnectedPlayer connectionOwner => _sessionManager.connectionOwner;

        public IConnectedPlayer GetPlayerByUserId(string userId) => _sessionManager.GetPlayerByUserId(userId);
        public void SetLocalPlayerState(string state, bool hasState) => _sessionManager.SetLocalPlayerState(state, hasState);
        public bool LocalPlayerHasState(string state) => _sessionManager.LocalPlayerHasState(state);
        public void Send<T>(T message) where T : INetSerializable => _sessionManager.Send(message);
    }
}
