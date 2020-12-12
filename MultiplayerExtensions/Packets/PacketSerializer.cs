using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Packets
{
    class PacketSerializer : INetworkPacketSubSerializer<IConnectedPlayer>
    {
        private Dictionary<string, Action<NetDataReader, int, IConnectedPlayer>> packetHandlers = new Dictionary<string, Action<NetDataReader, int, IConnectedPlayer>>();
        private List<Type> registeredTypes = new List<Type>();

        public void Serialize(NetDataWriter writer, INetSerializable packet)
        {
            writer.Put(packet.GetType().ToString());
            packet.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader, int length, IConnectedPlayer data)
        {
            int prevPosition = reader.Position;
            string packetType = reader.GetString();
            length -= reader.Position - prevPosition;
            Action<NetDataReader, int, IConnectedPlayer> action;
            if (this.packetHandlers.TryGetValue(packetType, out action))
            {
                if (action != null)
                {
                    action(reader, length, data);
                    return;
                }
            }
            else
            {
                reader.SkipBytes(length);
                return;
            }
        }

        public bool HandlesType(Type type)
        {
            return registeredTypes.Contains(type);
        }

        public void RegisterCallback<TPacket>(Action<TPacket> callback) where TPacket : INetSerializable, IPoolablePacket, new()
        {
            this.RegisterCallback<TPacket>(delegate (TPacket packet, IConnectedPlayer player)
            {
                callback?.Invoke(packet);
            });
        }

        public void RegisterCallback<TPacket>(Action<TPacket, IConnectedPlayer> callback) where TPacket : INetSerializable, IPoolablePacket, new()
        {
            this.registeredTypes.Add(typeof(TPacket));

            Func<NetDataReader, int, TPacket> deserialize = delegate (NetDataReader reader, int size)
            {
                TPacket packet = ThreadStaticPacketPool<TPacket>.pool.Obtain();
                if (packet == null)
                {
                    Plugin.Log?.Error($"(PacketSerializer) Constructor for '{typeof(TPacket)}' returned null!");
                    reader.SkipBytes(size);
                }
                else
                {
                    packet.Deserialize(reader);
                }
                return packet;
            };

            this.packetHandlers[typeof(TPacket).ToString()] = delegate (NetDataReader reader, int size, IConnectedPlayer player)
            {
                callback(deserialize(reader, size), player);
            };
        }
    }
}
