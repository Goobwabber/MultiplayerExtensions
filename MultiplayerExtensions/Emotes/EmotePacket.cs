using LiteNetLib.Utils;

namespace MultiplayerExtensions.Emotes
{
    class EmotePacket : INetSerializable, IPoolablePacket
    {
        public void Release() => ThreadStaticPacketPool<EmotePacket>.pool.Release(this);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(source);
        }

        public void Deserialize(NetDataReader reader)
        {
            source = reader.GetString();
        }

        public string source = null!;
    }
}
