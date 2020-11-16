using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Packets
{
    public class PacketManager : IInitializable
    {
        [Inject]
        private IMultiplayerSessionManager _sessionManager;

        private PacketSerializer _packetSerializer = new PacketSerializer();

        public void Initialize()
        {
            _sessionManager.RegisterSerializer((MultiplayerSessionManager.MessageType)100, _packetSerializer);
        }

        public void Send<T>(T message) where T : INetSerializable => _sessionManager.Send(message);

        public void RegisterSerializer(PacketSerializer serializer)
        {
            _packetSerializer.RegisterSerializer(serializer);
        }

        public void UnregisterSerializer(PacketSerializer serializer)
        {
            _packetSerializer.RegisterSerializer(serializer);
        }
    }
}
