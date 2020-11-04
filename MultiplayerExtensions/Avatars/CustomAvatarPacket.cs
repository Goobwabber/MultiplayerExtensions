using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Avatars
{
    public class CustomAvatarPacket : INetSerializable, IPoolablePacket
    {
        public static PacketPool<CustomAvatarPacket> pool
        {
            get
            {
                return ThreadStaticPacketPool<CustomAvatarPacket>.pool;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(hash);
            writer.Put(scale);
            writer.Put(floor);
        }

        public void Deserialize(NetDataReader reader)
        {
            hash = reader.GetString();
            scale = reader.GetFloat();
            floor = reader.GetFloat();
        }

        public void Release()
        {
            CustomAvatarPacket.pool.Release(this);
        }

        public CustomAvatarPacket Init(string avatarHash, float avatarScale, float avatarFloor)
        {
            this.hash = avatarHash;
            this.scale = avatarScale;
            this.floor = avatarFloor;

            return this;
        }

        public string hash;
        public float scale;
        public float floor;
    }
}
