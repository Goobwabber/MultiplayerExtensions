using LiteNetLib.Utils;
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
            writer.Put(ColorUtility.ToHtmlStringRGB(playerColor));
            writer.Put((int)platform);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.platformID = reader.GetString();
            this.mpexVersion = reader.GetString();

            if (!ColorUtility.TryParseHtmlString(reader.GetString(), out playerColor))
                this.playerColor = new Color(0.031f, 0.752f, 1f);

            //Plugin.Log.Warn($"AvailableBytes: {reader.AvailableBytes}");
            if (reader.AvailableBytes >= 4) // Verify this works when the platform int exists.
                this.platform = (Platform)reader.GetInt();
            else
                this.platform = Platform.Unknown;
        }

        public ExtendedPlayerPacket Init(string platformID, Platform platform, Color playerColor)
        {
            this.platformID = platformID;
            this.mpexVersion = Plugin.PluginMetadata.Version.ToString();
            this.playerColor = playerColor;
            this.platform = platform;
            return this;
        }

        public string platformID = null!;
        public Platform platform;
        public string mpexVersion = null!;
        public Color playerColor;
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
