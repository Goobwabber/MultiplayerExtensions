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
        private ConnectedPlayerManager? _playerManager => ((MultiplayerSessionManager)_sessionManager)?.GetField<ConnectedPlayerManager, MultiplayerSessionManager>("_connectedPlayerManager");
        private PacketSerializer _serializer = new PacketSerializer();

        private Func<IConnectedPlayer, object?> _getConnection;
        private Func<IConnectedPlayer, object?> GetConnection
        {
            get
            {
                if (_getConnection == null)
                {
                    MethodInfo getPlayer = typeof(ConnectedPlayerManager).GetMethod("GetPlayer", new Type[] { typeof(string) });
                    _getConnection = (IConnectedPlayer player) => _playerManager != null ? getPlayer.Invoke(_playerManager, new object[] { player.userId }) : null;
                }
                return _getConnection;
            }
        }

        private Action<object, object> _sendExcludingConnection;
        private Action<object, object> SendExcludingConnection
        {
            get
            {
                if (_sendExcludingConnection == null)
                {
                    MethodInfo sendExcludingPlayer = typeof(ConnectedPlayerManager).GetMethod("SendImmediatelyExcludingPlayer");
                    _sendExcludingConnection = (object message, object excludedConnection) => sendExcludingPlayer.Invoke(_playerManager, new object[] { message, excludedConnection, false });
                }
                return _sendExcludingConnection;
            }
        }

        private Action<object, object> _sendToConnection;
        private Action<object, object> SendToConnection
        {
            get
            {
                if (_sendToConnection == null)
                {
                    MethodInfo sendToPlayer = typeof(ConnectedPlayerManager).GetMethod("SendImmediatelyToPlayer");
                    _sendToConnection = (object message, object toConnection) => sendToPlayer.Invoke(_playerManager, new object[] { message, toConnection });
                }
                return _sendToConnection;
            }
        }

        public void Initialize()
        {
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

        public void SendImmediatelyExcludingPlayer<T>(T message, IConnectedPlayer excludedPlayer) where T : INetSerializable
        {
            object? excludedConnection = GetConnection(excludedPlayer);
            if (excludedConnection != null)
            {
                SendExcludingConnection(message, excludedConnection);
            }
            else
                Plugin.Log?.Error($"(PacketManager) '{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated!");
        }

        public void SendImmediatelyToPlayer<T>(T message, IConnectedPlayer toPlayer) where T : INetSerializable
        {
            object? toConnection = GetConnection(toPlayer);
            if (toConnection != null)
            {
                SendToConnection(message, toConnection);
            }
            else
                Plugin.Log?.Error($"(PacketManager) '{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated!");
        }
    }
}
