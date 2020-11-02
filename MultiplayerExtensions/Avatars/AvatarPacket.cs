using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Avatars
{
    class AvatarPacket : INetSerializable, IPoolablePacket
    {
        public static PacketPool<AvatarPacket> pool
        {
            get
            {
                return ThreadStaticPacketPool<AvatarPacket>.pool;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(avatarHash);
            writer.Put(avatarScale);
            writer.Put(avatarFloor);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.avatarHash = reader.GetString();
            this.avatarScale = reader.GetFloat();
            this.avatarFloor = reader.GetFloat();
        }

        public void Release()
        {
            AvatarPacket.pool.Release(this);
        }

        public AvatarPacket Init(string avatarHash, float avatarScale, float avatarFloor)
        {
            this.avatarHash = avatarHash;
            this.avatarScale = avatarScale;
            this.avatarFloor = avatarFloor;

            return this;
        }

        public string avatarHash;
        public float avatarScale;
        public float avatarFloor;
    }
}
