using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;

namespace MultiplayerExtensions.Packets
{
    class CustomPlayerPacket : INetSerializable, IPoolablePacket
    {
        public static PacketPool<CustomPlayerPacket> pool
        {
            get
            {
                return ThreadStaticPacketPool<CustomPlayerPacket>.pool;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(platformID);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.platformID = reader.GetString();
        }

        public void Release()
        {
            CustomPlayerPacket.pool.Release(this);
        }

        public CustomPlayerPacket Init(string platformID)
        {
            this.platformID = platformID;

            return this;
        }

        public string platformID;
    }
}
