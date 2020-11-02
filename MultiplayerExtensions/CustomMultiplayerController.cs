using BS_Utils.Utilities;
using LiteNetLib.Utils;
using MultiplayerExtensions.Avatars;
using MultiplayerExtensions.Controllers;
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

        private MultiplayerSessionManager _multiplayerSessionManager;
        private NetworkPacketSerializer<CustomMultiplayerController.MessageType, IConnectedPlayer> _packetSerializer = new NetworkPacketSerializer<CustomMultiplayerController.MessageType, IConnectedPlayer>();

        private Transform _container;

        public void Initialize()
        {
            Plugin.Log?.Info("Multiplayer Controller created.");
            _multiplayerSessionManager = Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();

            _multiplayerSessionManager.playerConnectedEvent += OnPlayerConnected;
            _multiplayerSessionManager.RegisterSerializer((MultiplayerSessionManager.MessageType)4, _packetSerializer);
            RegisterCallback(CustomMultiplayerController.MessageType.PlayerUpdate, HandlePlayerPacket, new Func<CustomPlayerPacket>(CustomPlayerPacket.pool.Obtain));

            _container = new GameObject(nameof(CustomMultiplayerController)).transform;
            UnityEngine.Object.DontDestroyOnLoad(_container);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_container);

            _multiplayerSessionManager.playerConnectedEvent -= OnPlayerConnected;
        }

        [Inject]
        internal void Inject()
        {
            
        }

        private void OnPlayerConnected(IConnectedPlayer player)
        {
            string platformId = BS_Utils.Gameplay.GetUserInfo.GetUserID() ?? "null";
            Send(new CustomPlayerPacket().Init(platformId));
            connectedEvent();
        }

        private void HandlePlayerPacket(CustomPlayerPacket packet, IConnectedPlayer player)
        {
            Plugin.Log?.Info($"{player.userName} platform id: {packet.platformID}");
        }

        public void Send<T>(T message) where T : INetSerializable
        {
            string data = message switch
            {
                CustomPlayerPacket cpp => (message as CustomPlayerPacket).platformID,
                AvatarPacket ap => (message as AvatarPacket).avatarHash,
                _ => "null",
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
    }
}
