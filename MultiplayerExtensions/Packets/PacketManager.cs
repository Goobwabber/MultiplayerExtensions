using IPA.Utilities;
using LiteNetLib.Utils;
using System;
using Zenject;

namespace MultiplayerExtensions.Packets
{
    public class PacketManager : IInitializable, IDisposable
    {
        //private delegate object GetConnection(ref ConnectedPlayerManager instance, string userId);
        //private delegate void SendImmediate(ref ConnectedPlayerManager instance, INetSerializable message, bool onlyFirstDegree);
        //private delegate void SendExcluding(ref ConnectedPlayerManager instance, INetSerializable message, object excludingPlayer, bool onlyFirstDegree);
        //private delegate void SendToPlayer(ref ConnectedPlayerManager instance, INetSerializable message, object toPlayer);

        //private static GetConnection _getConnection = MethodAccessor<ConnectedPlayerManager, GetConnection>.GetDelegate("GetPlayer");
        //private static SendImmediate _sendImmediate = MethodAccessor<ConnectedPlayerManager, SendImmediate>.GetDelegate("SendImmediately");
        //private static SendExcluding _sendExcluding = MethodAccessor<ConnectedPlayerManager, SendExcluding>.GetDelegate("SendImmediatelyExcludingPlayer");
        //private static SendToPlayer _sendToPlayer = MethodAccessor<ConnectedPlayerManager, SendToPlayer>.GetDelegate("SendImmediatelyToPlayer");

        private static FieldAccessor<MultiplayerSessionManager, ConnectedPlayerManager>.Accessor _playerManager
            = FieldAccessor<MultiplayerSessionManager, ConnectedPlayerManager>.GetAccessor("_connectedPlayerManager");

        private MultiplayerSessionManager _sessionManager;
        private PacketSerializer _serializer = new PacketSerializer();

        internal PacketManager(IMultiplayerSessionManager sessionManager)
        {
            _sessionManager = (sessionManager as MultiplayerSessionManager)!;
        }

        public void Initialize()
        {
            _sessionManager.RegisterSerializer((MultiplayerSessionManager.MessageType)100, _serializer);
        }

        public void Dispose()
        {
            _sessionManager.UnregisterSerializer((MultiplayerSessionManager.MessageType)100, _serializer);
        }

        /// <summary>
        /// Sends a <see cref="LiteNetLib.Utils.INetSerializable"/> object to all other players in the lobby.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <seealso cref="PacketManager.SendUnreliable(INetSerializable)"/>
        public void Send(INetSerializable message) 
            => _sessionManager.Send(message);

        /// <summary>
        /// Sends an unreliable packet with a <see cref="INetSerializable"/> object to all other players in the lobby.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <seealso cref="PacketManager.Send(INetSerializable)"/>
        public void SendUnreliable(INetSerializable message) 
            => _sessionManager.SendUnreliable(message);

        /// <summary>
        /// Registers a callback that is fired when a <typeparamref name="T"/> packet is recieved.
        /// </summary>
        /// <typeparam name="T">Type of packet to listen for. Inherits <see cref="LiteNetLib.Utils.INetSerializable"/> and <see cref="IPoolablePacket"/></typeparam>
        /// <param name="action">Action that handles recieved packet.</param>
        /// <seealso cref="PacketManager.RegisterCallback{T}(Action{T, IConnectedPlayer})"/>
        /// <seealso cref="PacketManager.UnregisterCallback{T}"/>
        public void RegisterCallback<T>(Action<T> action) where T : INetSerializable, IPoolablePacket, new() 
            => _serializer.RegisterCallback<T>(action);

        /// <summary>
        /// Registers a callback that is fired when a <typeparamref name="T"/> packet is received.
        /// </summary>
        /// <typeparam name="T">Type of packet to listen for. Inherits <see cref="LiteNetLib.Utils.INetSerializable"/> and <see cref="IPoolablePacket"/></typeparam>
        /// <param name="action">Action that handles recieved packet and player.</param>
        /// <seealso cref="RegisterCallback{T}(Action{T})"/>
        /// <seealso cref="PacketManager.UnregisterCallback{T}"/>
        public void RegisterCallback<T>(Action<T, IConnectedPlayer> action) where T : INetSerializable, IPoolablePacket, new() 
            => _serializer.RegisterCallback<T>(action);

        /// <summary>
        /// Unregisters a <typeparamref name="T"/> packet.
        /// </summary>
        /// <typeparam name="T">Type of packet to stop listening for. Inherits <see cref="LiteNetLib.Utils.INetSerializable"/> and <see cref="IPoolablePacket"/></typeparam>
        /// <seealso cref="RegisterCallback{T}(Action{T})"/>
        /// <seealso cref="PacketManager.RegisterCallback{T}(Action{T, IConnectedPlayer})"/>
        public void UnregisterCallback<T>() where T : INetSerializable, IPoolablePacket, new() 
            => _serializer.UnregisterCallback<T>();

        /// <summary>
        /// Sends a <typeparamref name="T"/> packet to all players without waiting for next flush.
        /// </summary>
        /// <remarks>
        /// Can cause crashes if used improperly.
        /// </remarks>
        /// <exception cref="System.NullReferenceException">Thrown when <see cref="ConnectedPlayerManager"/> has not been instantiated.</exception>
        /// <typeparam name="T">Type of <see cref="LiteNetLib.Utils.INetSerializable"/> to send.</typeparam>
        /// <param name="message"><see cref="LiteNetLib.Utils.INetSerializable"/> packet to send.</param>
        //public void SendImmediately<T>(T message) where T : INetSerializable
        //{
        //    ConnectedPlayerManager playerManager = _playerManager(ref _sessionManager);
        //    if (playerManager != null)
        //        _sendImmediate.Invoke(ref playerManager, message, false);
        //    else
        //        throw new NullReferenceException($"'{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated.");
        //}

        /// <summary>
        /// Sends a <typeparamref name="T"/> packet to all players except <paramref name="excludedPlayer"/> without waiting for next flush.
        /// </summary>
        /// <remarks>
        /// Can cause crashes if used improperly.
        /// </remarks>
        /// <exception cref="System.NullReferenceException">Thrown when <see cref="ConnectedPlayerManager"/> has not been instantiated.</exception>
        /// <exception cref="System.NullReferenceException">Thrown when <paramref name="excludedPlayer"/> cannot be found.</exception>
        /// <typeparam name="T">Type of <see cref="LiteNetLib.Utils.INetSerializable"/> to send.</typeparam>
        /// <param name="message"><see cref="LiteNetLib.Utils.INetSerializable"/> packet to send.</param>
        /// <param name="excludedPlayer"><see cref="IConnectedPlayer"/> that packet won't be sent to.</param>
        //public void SendImmediatelyExcludingPlayer<T>(T message, IConnectedPlayer excludedPlayer) where T : INetSerializable
        //{
        //    ConnectedPlayerManager playerManager = _playerManager(ref _sessionManager);
        //    if (playerManager != null)
        //    {
        //        //object excludedConnection = _getConnection(ref playerManager, excludedPlayer.userId);
        //        //if (excludedConnection != null)
        //        //    _sendExcluding.Invoke(ref playerManager, message, excludedConnection, false);
        //        //else
        //        //    throw new NullReferenceException($"'{typeof(T)}' was not sent because user '{excludedPlayer.userId}' could not be found.");
        //    } else
        //        throw new NullReferenceException($"'{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated.");
        //}

        /// <summary>
        /// Sends a <typeparamref name="T"/> packet to a <paramref name="toPlayer"/> without waiting for next flush.
        /// </summary>
        /// <remarks>
        /// Can cause crashes if used improperly.
        /// </remarks>
        /// <exception cref="System.NullReferenceException">Thrown when <see cref="ConnectedPlayerManager"/> has not been instantiated.</exception>
        /// <exception cref="System.NullReferenceException">Thrown when <paramref name="toPlayer"/> cannot be found.</exception>
        /// <typeparam name="T">Type of <see cref="LiteNetLib.Utils.INetSerializable"/> to send.</typeparam>
        /// <param name="message"><see cref="LiteNetLib.Utils.INetSerializable"/> packet to send.</param>
        /// <param name="toPlayer"><see cref="IConnectedPlayer"/> to send the packet to.</param>
        //public void SendImmediatelyToPlayer<T>(T message, IConnectedPlayer toPlayer) where T : INetSerializable
        //{
        //    ConnectedPlayerManager playerManager = _playerManager(ref _sessionManager);
        //    if (playerManager != null)
        //    {
        //        //object toConnection = _getConnection(ref playerManager, toPlayer.userId);
        //        //if (toConnection != null)
        //        //    _sendToPlayer.Invoke(ref playerManager, message, toConnection);
        //        //else
        //        //    throw new NullReferenceException($"'{typeof(T)}' was not sent because user '{toPlayer.userId}' could not be found.");
        //    }
        //    else
        //        throw new NullReferenceException($"'{typeof(T)}' was not sent because 'ConnectedPlayerManager' has not been instantiated.");
        //}
    }
}
