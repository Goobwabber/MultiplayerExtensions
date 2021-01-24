using LiteNetLib.Utils;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Sessions
{
    class ExtendedPlayerPacket : INetSerializable, IPoolablePacket
    {
        public void Release() => ThreadStaticPacketPool<ExtendedPlayerPacket>.pool.Release(this);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(platformID);
            writer.Put(mpexVersion);
            playerColor.Serialize(writer);
            writer.Put((int)platform);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.platformID = reader.GetString();
            this.mpexVersion = reader.GetString();
            playerColor.Deserialize(reader);
            Plugin.Log.Warn($"AvailableBytes: {reader.AvailableBytes}");
            if (reader.AvailableBytes >= 4) // Verify this works when the platform int exists.
                this.platform = (Platform)reader.GetInt();
            else
                this.platform = Platform.Unknown;
        }

        public ExtendedPlayerPacket Init(string platformID, Platform platform, Color playerColor)
        {
            this.platformID = platformID;
            this.mpexVersion = Plugin.PluginMetadata.Version.ToString();
            this.playerColor = new Color32Serializable(playerColor);
            return this;
        }

        public string platformID;
        public Platform platform;
        public string mpexVersion;
        public Color32Serializable playerColor;
    }

    public enum Platform
    {
        Unknown = 0,
        Steam = 1,
        OculusPC = 2,
        OculusQuest = 3,
        PS4 = 4
    }
}
