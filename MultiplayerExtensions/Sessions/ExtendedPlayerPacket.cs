using LiteNetLib.Utils;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Sessions
{
    class ExtendedPlayerPacket : INetSerializable, IPoolablePacket
    {
        public void Release() => ThreadStaticPacketPool<ExtendedPlayerPacket>.pool.Release(this);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(platformID);
            writer.Put(mpexVersion);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.platformID = reader.GetString();
            this.mpexVersion = reader.GetString();
        }

        public ExtendedPlayerPacket Init(string platformID)
        {
            this.platformID = platformID;
            this.mpexVersion = Plugin.PluginMetadata.Version.ToString();

            return this;
        }

        public string platformID;
        public string mpexVersion;
    }
}
