using LiteNetLib.Utils;

namespace MultiplayerExtensions.Extensions
{
	class ExtendedPlayerReadyPacket : INetSerializable, IPoolablePacket
	{
        public void Release() => ThreadStaticPacketPool<ExtendedPlayerReadyPacket>.pool.Release(this);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ready);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.ready = reader.GetBool();
        }

        public ExtendedPlayerReadyPacket Init(bool ready)
		{
            this.ready = ready;
            return this;
		}

        public bool ready;
    }
}
