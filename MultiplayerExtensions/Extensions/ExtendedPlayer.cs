using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerExtensions.Extensions
{
    public class ExtendedPlayer : IConnectedPlayer
    {
        public static readonly Color DefaultColor = new Color(0.031f, 0.752f, 1f);

        private IConnectedPlayer _connectedPlayer;

        /// <summary>
        /// Platform User ID from <see cref="UserInfo.platformUserId"/>
        /// </summary>
        public string platformID { get; internal set; } = null!;

        /// <summary>
        /// Platform from <see cref="UserInfo.platformUserId">
        /// </summary>
        public Platform platform { get; internal set; }

        /// <summary>
        /// MultiplayerExtensions version reported by BSIPA.
        /// </summary>
        public Hive.Versioning.Version mpexVersion;

        /// <summary>
        /// Player's color set in the plugin config.
        /// </summary>
        public Color playerColor;

        public bool isPartyOwner { get; internal set; }
        public bool hasRecommendBeatmapPermission { get; internal set; }
        public bool hasRecommendModifiersPermission { get; internal set; }
        public bool hasKickVotePermission { get; internal set; }

        public ExtendedPlayer(IConnectedPlayer player)
		{
            _connectedPlayer = player;
            this.mpexVersion = Plugin.ProtocolVersion;
        }

        public ExtendedPlayer(IConnectedPlayer player, string platformID, Platform platform, Color playerColor)
		{
            _connectedPlayer = player;
            this.platformID = platformID;
            this.platform = platform;
            this.mpexVersion = Plugin.ProtocolVersion;
            this.playerColor = playerColor;
        }

        public ExtendedPlayer(IConnectedPlayer player, string platformID, Platform platform, Hive.Versioning.Version mpexVersion, Color playerColor)
        {
            _connectedPlayer = player;
            this.platformID = platformID;
            this.platform = platform;
            this.mpexVersion = mpexVersion;
            this.playerColor = playerColor;
        }

        public bool isMe => _connectedPlayer.isMe;
        public string userId => _connectedPlayer.userId;
        public string userName => _connectedPlayer.userName;
        public float currentLatency => _connectedPlayer.currentLatency;
        public bool isConnected => _connectedPlayer.isConnected;
        public bool isConnectionOwner => _connectedPlayer.isConnectionOwner;
        public float offsetSyncTime => _connectedPlayer.offsetSyncTime;
        public int sortIndex => _connectedPlayer.sortIndex;
        public bool isKicked => _connectedPlayer.isKicked;
        public MultiplayerAvatarData multiplayerAvatarData => _connectedPlayer.multiplayerAvatarData;
        public DisconnectedReason disconnectedReason => _connectedPlayer.disconnectedReason;
        public bool HasState(string state) => _connectedPlayer.HasState(state);
    }

    public class ExtendedPlayerPacket : INetSerializable, IPoolablePacket
    {
        public void Release() => ThreadStaticPacketPool<ExtendedPlayerPacket>.pool.Release(this);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(platformID);
            writer.Put(mpexVersion);
            writer.Put("#" + ColorUtility.ToHtmlStringRGB(playerColor));
            writer.Put((int)platform);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.platformID = reader.GetString();
            this.mpexVersion = reader.GetString();

            if (!ColorUtility.TryParseHtmlString(reader.GetString(), out playerColor))
                this.playerColor = ExtendedPlayer.DefaultColor;

            //Plugin.Log.Warn($"AvailableBytes: {reader.AvailableBytes}");
            if (reader.AvailableBytes >= 4) // Verify this works when the platform int exists.
                this.platform = (Platform)reader.GetInt();
            else
                this.platform = Platform.Unknown;
        }

        public ExtendedPlayerPacket Init(string platformID, Platform platform, Color playerColor)
        {
            this.platformID = platformID;
            this.mpexVersion = Plugin.ProtocolVersion.ToString();
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
