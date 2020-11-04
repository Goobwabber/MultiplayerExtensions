using MultiplayerExtensions.Avatars;

namespace MultiplayerExtensions.Networking
{
    class ExtendedPlayer : IConnectedPlayer
    {
        private IConnectedPlayer _connectedPlayer;
        public string? platformID;
        public CustomAvatarData? avatar;

        public ExtendedPlayer(IConnectedPlayer player)
        {
            _connectedPlayer = player;
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
        public bool HasState(string state) => _connectedPlayer.HasState(state);
    }
}
