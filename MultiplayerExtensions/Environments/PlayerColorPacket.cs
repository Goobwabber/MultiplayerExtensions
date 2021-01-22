using LiteNetLib.Utils;

namespace MultiplayerExtensions.Environments
{
    class PlayerColorPacket : INetSerializable, IPoolablePacket
    {
        public string color { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(color);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.color = reader.GetString();
        }

        public void Release()
            => ThreadStaticPacketPool<PlayerColorPacket>.pool.Release(this);
    }
}
