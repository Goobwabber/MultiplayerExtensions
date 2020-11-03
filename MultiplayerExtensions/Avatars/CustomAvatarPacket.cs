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
            writer.Put(avatarHash);
            writer.Put(avatarScale);
            writer.Put(avatarFloor);
            writer.Put(avatarPelvis);
        }

        public void Deserialize(NetDataReader reader)
        {
            avatarHash = reader.GetString();
            avatarScale = reader.GetFloat();
            avatarFloor = reader.GetFloat();
            avatarPelvis = reader.GetBool();
        }

        public void Release()
        {
            CustomAvatarPacket.pool.Release(this);
        }

        public CustomAvatarPacket Init(string avatarHash, float avatarScale, float avatarFloor, bool avatarPelvis)
        {
            this.avatarHash = avatarHash;
            this.avatarScale = avatarScale;
            this.avatarFloor = avatarFloor;
            this.avatarPelvis = avatarPelvis;

            return this;
        }

        public string avatarHash;
        public float avatarScale;
        public float avatarFloor;
        public bool avatarPelvis;
    }
}
