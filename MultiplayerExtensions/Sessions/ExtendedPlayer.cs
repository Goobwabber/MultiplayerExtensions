using UnityEngine;

namespace MultiplayerExtensions.Sessions
{
    public class ExtendedPlayer : IConnectedPlayer
    {
        private IConnectedPlayer _connectedPlayer;

        /// <summary>
        /// Platform User ID from <see cref="UserInfo.platformUserId"/>
        /// </summary>
        public string platformID { get; internal set; }

        /// <summary>
        /// Platform from <see cref="UserInfo.platformUserId">
        /// </summary>
        public Platform platform { get; internal set; }

        /// <summary>
        /// MultiplayerExtensions version reported by BSIPA.
        /// </summary>
        public SemVer.Version mpexVersion;

        /// <summary>
        /// Player's color set in the plugin config.
        /// </summary>
        public Color playerColor;

        internal GameplayModifiers? lastModifiers;

        public ExtendedPlayer(IConnectedPlayer player, string platformID, Platform platform, SemVer.Version mpexVersion, Color playerColor)
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
}
