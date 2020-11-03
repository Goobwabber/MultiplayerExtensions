using BS_Utils.Utilities;
using LiteNetLib.Utils;
using MultiplayerExtensions.Avatars;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions
{
    internal class CustomMultiplayerController : IInitializable, IDisposable
    {
        public Action connectedEvent;
        public Action disconnectedEvent;

        public Dictionary<string, CustomPlayer> players = new Dictionary<string, CustomPlayer>();

        private MultiplayerSessionManager _multiplayerSessionManager;
        private NetworkPacketSerializer<CustomMultiplayerController.MessageType, IConnectedPlayer> _packetSerializer = new NetworkPacketSerializer<CustomMultiplayerController.MessageType, IConnectedPlayer>();

        private Transform _container;

        public void Initialize()
        {
            Plugin.Log?.Info("Multiplayer Controller created.");
            _multiplayerSessionManager = Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();

            connectedEvent += SendPlayerPacket;
            _multiplayerSessionManager.playerConnectedEvent += OnPlayerConnected;
            _multiplayerSessionManager.playerDisconnectedEvent += OnPlayerDisconnected;
            _multiplayerSessionManager.RegisterSerializer((MultiplayerSessionManager.MessageType)4, _packetSerializer);
            RegisterCallback(CustomMultiplayerController.MessageType.PlayerUpdate, HandlePlayerPacket, new Func<CustomPlayerPacket>(CustomPlayerPacket.pool.Obtain));

            _container = new GameObject(nameof(CustomMultiplayerController)).transform;
            UnityEngine.Object.DontDestroyOnLoad(_container);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_container);

            _multiplayerSessionManager.playerConnectedEvent -= OnPlayerConnected;
            _multiplayerSessionManager.playerDisconnectedEvent -= OnPlayerDisconnected;
        }

        [Inject]
        internal void Inject()
        {
            
        }

        private void OnPlayerConnected(IConnectedPlayer player)
        {
            players[player.userId] = new CustomPlayer(player);
            connectedEvent();
        }

        private void OnPlayerDisconnected(IConnectedPlayer player)
        {
            disconnectedEvent();
            players.Remove(player.userId);
        }

        private void SendPlayerPacket()
        {
            BS_Utils.Gameplay.GetUserInfo.GetUserAsync().ContinueWith(r =>
            {
                Send(new CustomPlayerPacket().Init(r.Result.platformUserId));
            });
        }

        private void HandlePlayerPacket(CustomPlayerPacket packet, IConnectedPlayer player)
        {
            var customPlayer = players[player.userId];
            customPlayer.platformID = packet.platformID;
        }

        public void Send<T>(T message) where T : INetSerializable
        {
            string data = message switch
            {
                CustomPlayerPacket cpp => (message as CustomPlayerPacket).platformID,
                CustomAvatarPacket cap => (message as CustomAvatarPacket).avatarHash,
                _ => "unknown",
            };

            Plugin.Log?.Info($"Sending {message.GetType().Name} packet with {data}");

            MultiplayerSessionManager multiplayerSessionManager = this._multiplayerSessionManager;
            if (multiplayerSessionManager == null)
            {
                return;
            }

            multiplayerSessionManager.Send(message);
        }

        public void RegisterCallback<T>(CustomMultiplayerController.MessageType serializerType, Action<T, IConnectedPlayer> callback, Func<T> constructor) where T : INetSerializable
        {
            _packetSerializer.RegisterCallback(serializerType, callback, constructor);
        }

        public enum MessageType : Byte
        {
            PlayerUpdate,
            AvatarUpdate
        }

        public class CustomPlayer : IConnectedPlayer
        {
            private IConnectedPlayer _connectedPlayer;
            public string? platformID;
            public CustomAvatarData customAvatar;

            public CustomPlayer(IConnectedPlayer player)
            {
                _connectedPlayer = player;
            }

            public bool isMe => _connectedPlayer.isMe;
            public string userId => _connectedPlayer.userId;
            public string userName => _connectedPlayer.userName;
            public float currentLatency => _connectedPlayer.currentLatency;
            public bool isConnected => _connectedPlayer.isConnected;
            public bool isConnectionOwner => _connectedPlayer.isConnectionOwner;
            public float offsetSyncTime => _connectedPlayer.offsetSyncTime;
            public int sortIndex => _connectedPlayer.sortIndex;
            public bool isKicked => _connectedPlayer.isKicked;
            public MultiplayerAvatarData multiplayerAvatarData => _connectedPlayer.multiplayerAvatarData;
            public bool HasState(string state) => _connectedPlayer.HasState(state);
        }
    }
}
