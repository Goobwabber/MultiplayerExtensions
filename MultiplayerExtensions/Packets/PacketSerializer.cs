using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Packets
{
    public class PacketSerializer : NetworkPacketSerializer<byte, IConnectedPlayer>
    {
        private Registry<Type> _packets = new Registry<Type>(256);
        private Registry<object> _serializers = new Registry<object>(256);

        public void RegisterCallback<T>(Action<T, IConnectedPlayer> callback, Func<T> constructor) where T : INetSerializable
        {
            if (!_packets.Contains(typeof(T)))
                _packets.Register(typeof(T));
            byte packetType = (byte)_packets.IndexOf(typeof(T));

            base.RegisterCallback<T>(packetType, callback, constructor);
        }

        public void UnregisterCallback<T>() where T : INetSerializable
        {
            if (!_packets.Contains(typeof(T)))
                throw new InvalidOperationException();
            byte packetType = (byte)_packets.IndexOf(typeof(T));

            base.UnregisterCallback<T>(packetType);
            _packets.Unregister(typeof(T));
        }

        public void RegisterSerializer(INetworkPacketSubSerializer<IConnectedPlayer> subSerializer)
        {
            if (!_serializers.Contains(subSerializer))
                _serializers.Register(subSerializer);
            byte packetType = (byte)_serializers.IndexOf(subSerializer);

            base.RegisterSubSerializer(packetType, subSerializer);
        }

        public void UnregisterSerializer(INetworkPacketSubSerializer<IConnectedPlayer> subSerializer)
        {
            if (!_serializers.Contains(subSerializer))
                throw new InvalidOperationException();
            byte packetType = (byte)_serializers.IndexOf(subSerializer);

            base.RegisterSubSerializer(packetType, subSerializer);
        }
    }
}
