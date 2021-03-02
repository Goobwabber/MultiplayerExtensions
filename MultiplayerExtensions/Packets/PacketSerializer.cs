using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

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
            prevPosition = reader.Position;

            Action<NetDataReader, int, IConnectedPlayer> action;
            if (packetHandlers.TryGetValue(packetType, out action) && action != null) {
                try {
                    action(reader, length, data);
                } catch (Exception ex) {
                    Plugin.Log?.Warn($"An exception was thrown processing custom packet '{packetType}' from player '{data?.userName ?? "<NULL>"}|{data?.userId ?? " < NULL > "}': {ex.Message}");
                    Plugin.Log?.Debug(ex);
                }
            }
            
            // skip any unprocessed bytes (or rewind the reader if too many bytes were read)
            int processedBytes = reader.Position - prevPosition;
            reader.SkipBytes(length - processedBytes);
        }

        public bool HandlesType(Type type)
        {
            return registeredTypes.Contains(type);
        }

        internal void RegisterCallback<TPacket>(Action<TPacket> callback) where TPacket : INetSerializable, IPoolablePacket, new()
        {
            RegisterCallback<TPacket>(delegate (TPacket packet, IConnectedPlayer player)
            {
                callback?.Invoke(packet);
            });
        }

        internal void RegisterCallback<TPacket>(Action<TPacket, IConnectedPlayer> callback) where TPacket : INetSerializable, IPoolablePacket, new()
        {
            registeredTypes.Add(typeof(TPacket));

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
#pragma warning disable CS8603 // Possible null reference return. Null isn't possible, but ok
                return packet;
#pragma warning restore CS8603 // Possible null reference return.
            };

            packetHandlers[typeof(TPacket).ToString()] = delegate (NetDataReader reader, int size, IConnectedPlayer player)
            {
                callback(deserialize(reader, size), player);
            };
        }

        internal void UnregisterCallback<TPacket>() where TPacket : INetSerializable, IPoolablePacket, new()
            => packetHandlers.Remove(typeof(TPacket).ToString());
    }
}
