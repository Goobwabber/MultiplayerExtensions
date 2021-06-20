using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerExtensions.Emotes
{
    internal class EmotePacket : INetSerializable, IPoolablePacket
    {
        public void Release() => ThreadStaticPacketPool<EmotePacket>.pool.Release(this);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(source);

            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);

            writer.Put(rotation.x);
            writer.Put(rotation.y);
            writer.Put(rotation.z);
            writer.Put(rotation.w);
        }

        public void Deserialize(NetDataReader reader)
        {
            source = reader.GetString();

            position.x = reader.GetFloat();
            position.y = reader.GetFloat();
            position.z = reader.GetFloat();

            rotation.x = reader.GetFloat();
            rotation.y = reader.GetFloat();
            rotation.z = reader.GetFloat();
            rotation.w = reader.GetFloat();
        }

        public string source = null!;
        public Vector3 position;
        public Quaternion rotation;
    }
}
