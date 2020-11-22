using IPA.Utilities;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Packets
{
    public class PacketManager : IInitializable
    {
        [Inject]
        private IMultiplayerSessionManager _sessionManager;
        private MultiplayerSessionManager _multiplayerSessionManager;
        private ConnectedPlayerManager? _playerManager => _multiplayerSessionManager.GetField<ConnectedPlayerManager, MultiplayerSessionManager>("_connectedPlayerManager");
        private PacketSerializer _serializer = new PacketSerializer();

        public void Initialize()
        {
            _multiplayerSessionManager = Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();
            _sessionManager.RegisterSerializer((MultiplayerSessionManager.MessageType)100, _serializer);
        }

        public void Send(INetSerializable message) => _sessionManager.Send(message);
        public void SendUnreliable(INetSerializable message) => _sessionManager.SendUnreliable(message);

        public void RegisterCallback<T>(Action<T> action) where T : INetSerializable, IPoolablePacket, new() => _serializer.RegisterCallback<T>(action);
        public void RegisterCallback<T>(Action<T, IConnectedPlayer> action) where T : INetSerializable, IPoolablePacket, new() => _serializer.RegisterCallback<T>(action);

        public void SendImmediately<T>(T message) where T : INetSerializable
        {
            MethodInfo sendImmediately = typeof(ConnectedPlayerManager).GetMethod("SendImmediately");
            if (_playerManager != null)
                sendImmediately.Invoke(_playerManager, new object[] { message, false });
            else
                Plugin.Log?.Error($"(PacketManager) '{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated!");
        }

        private object? GetConnection(IConnectedPlayer player)
        {
            MethodInfo getPlayer = typeof(ConnectedPlayerManager).GetMethod("GetPlayer", new Type[] { typeof(string) });
            return _playerManager != null ? getPlayer.Invoke(_playerManager, new object[] { player.userId }) : null;
        }

        public void SendImmediatelyExcludingPlayer<T>(T message, IConnectedPlayer excludedPlayer) where T : INetSerializable
        {
            object? excludedConnection = GetConnection(excludedPlayer);
            if (excludedConnection != null)
            {
                MethodInfo sendExcludingPlayer = typeof(ConnectedPlayerManager).GetMethod("SendImmediatelyExcludingPlayer");
                sendExcludingPlayer.Invoke(_playerManager, new object[] { message, excludedConnection, false });
            }
            else
                Plugin.Log?.Error($"(PacketManager) '{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated!");
        }

        public void SendImmediatelyToPlayer<T>(T message, IConnectedPlayer toPlayer) where T : INetSerializable
        {
            object? toConnection = GetConnection(toPlayer);
            if (toConnection != null)
            {
                MethodInfo sendToPlayer = typeof(ConnectedPlayerManager).GetMethod("SendImmediatelyToPlayer");
                sendToPlayer.Invoke(_playerManager, new object[] { message, toPlayer });
            }
            else
                Plugin.Log?.Error($"(PacketManager) '{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated!");
        }
    }
}
