using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Networking
{
    public class ExtendedPlayerPacket : INetSerializable, IPoolablePacket
    {
        public static PacketPool<ExtendedPlayerPacket> pool
        {
            get
            {
                return ThreadStaticPacketPool<ExtendedPlayerPacket>.pool;
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
            ExtendedPlayerPacket.pool.Release(this);
        }

        public ExtendedPlayerPacket Init(string platformID)
        {
            this.platformID = platformID;

            return this;
        }

        public string platformID;
    }
}
