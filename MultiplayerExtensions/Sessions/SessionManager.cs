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
        private IMenuRpcManager _menuRpcManager = null!;

        [Inject]
        private BeatmapCharacteristicCollectionSO _beatmapCharacteristicCollection = null!;

        [Inject]
        private IMultiplayerSessionManager _sessionManager = null!;

        [Inject]
        private PacketManager _packetManager = null!;

        public void Initialize()
        {
            Plugin.Log?.Info("Setting up SessionManager");

            _sessionManager.SetLocalPlayerState("modded", true);
            _sessionManager.SetLocalPlayerState("customsongs", Plugin.Config.CustomSongs);
            _sessionManager.SetLocalPlayerState("enforcemods", Plugin.Config.EnforceMods);

            _sessionManager.connectedEvent += delegate () { connectedEvent?.Invoke(); };
            _sessionManager.connectionFailedEvent += delegate (ConnectionFailedReason reason) { connectionFailedEvent?.Invoke(reason); };
            _sessionManager.playerConnectedEvent += delegate (IConnectedPlayer player) { playerConnectedEvent?.Invoke(player); };
            _sessionManager.playerDisconnectedEvent += delegate (IConnectedPlayer player) { playerDisconnectedEvent?.Invoke(player); };
            _sessionManager.playerStateChangedEvent += delegate (IConnectedPlayer player) { playerStateChangedEvent?.Invoke(player); };
            _sessionManager.disconnectedEvent += delegate (DisconnectedReason reason) { disconnectedEvent?.Invoke(reason); };
            RemoveMenuRpcEvents();
            SetMenuRpcEvents();
        }

        private void RemoveMenuRpcEvents()
        {
            _menuRpcManager.selectedBeatmapEvent -= OnSelectedBeatmap;
            _menuRpcManager.clearSelectedBeatmapEvent -= OnSelectedBeatmapCleared;
        }

        private void SetMenuRpcEvents()
        {
            // TODO: Appears to stop firing after scene change?
            _menuRpcManager.selectedBeatmapEvent += OnSelectedBeatmap;
            _menuRpcManager.clearSelectedBeatmapEvent += OnSelectedBeatmapCleared;
        }

        private void OnSelectedBeatmapCleared(string userId)
        {
            OnSelectedBeatmap(userId, null);
        }

        private void OnSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable? beatmapId)
        {
            SelectedBeatmapEventArgs args;
            UserType userType = UserType.None;
            IConnectedPlayer? player = GetPlayerByUserId(userId);
            if (player != null)
            {
                if (player.isMe)
                    userType |= UserType.Local;
                if (player.isConnectionOwner)
                    userType |= UserType.Host;
            }
            else
                Plugin.Log.Warn($"OnSelectedBeatmap raised by an unknown player: {userId}. Selected '{beatmapId?.levelID ?? "<NULL>"}'");
            if (beatmapId == null || string.IsNullOrEmpty(beatmapId.levelID))
            {
                args = new SelectedBeatmapEventArgs(userId, userType);
            }
            else
            {
                BeatmapCharacteristicSO? characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                if (characteristic == null)
                    Plugin.Log?.Warn($"Unknown characteristic: '{beatmapId.beatmapCharacteristicSerializedName}'");
                args = new SelectedBeatmapEventArgs(userId, userType, beatmapId.levelID, beatmapId.difficulty, characteristic);
            }
            MPEvents.RaiseBeatmapSelected(this, args);
        }

        public event Action? connectedEvent;
        public event Action<ConnectionFailedReason>? connectionFailedEvent;
        public event Action<IConnectedPlayer>? playerConnectedEvent;
        public event Action<IConnectedPlayer>? playerDisconnectedEvent;
        public event Action<IConnectedPlayer>? playerStateChangedEvent;
        public event Action<DisconnectedReason>? disconnectedEvent;

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
