using CustomAvatar.Avatar;
using CustomAvatar.Player;
using MultiplayerExtensions.Networking;
using System;
using System.Threading;
using Zenject;

namespace MultiplayerExtensions.Avatars
{
    class CustomAvatarManager : IInitializable
    {
        [Inject]
        private ExtendedSessionManager _sessionManager;

        [Inject]
        private AvatarSpawner _avatarSpawner;

        [Inject]
        private PlayerAvatarManager _avatarManager;

        [Inject]
        private VRPlayerInput _playerInput;

        [Inject]
        private FloorController _floorController;

        [Inject]
        private IAvatarProvider<LoadedAvatar> _avatarProvider;

        public Action<ExtendedPlayer> avatarReceived;
        public CustomAvatarData localAvatar = new CustomAvatarData();

        public void Initialize()
        {
            Plugin.Log?.Info("Setting up CustomAvatarManager");

            _avatarManager.avatarChanged += OnAvatarChanged;
            _avatarManager.avatarScaleChanged += delegate(float scale) { localAvatar.scale = scale; };
            _floorController.floorPositionChanged += delegate (float floor) { localAvatar.floor = floor; };

            _sessionManager.connectedEvent += SendLocalAvatarPacket;
            _sessionManager.playerConnectedEvent += OnPlayerConnected;
            _sessionManager.RegisterCallback(ExtendedSessionManager.MessageType.AvatarUpdate, HandleAvatarPacket, new Func<CustomAvatarPacket>(CustomAvatarPacket.pool.Obtain));

            OnAvatarChanged(_avatarManager.currentlySpawnedAvatar);
            localAvatar.floor = _floorController.floorPosition;
        }

        private void OnAvatarChanged(SpawnedAvatar avatar)
        {
            if (!avatar) return;
            _avatarProvider.HashAvatar(avatar.avatar).ContinueWith(r =>
            {
                localAvatar.hash = r.Result;
                localAvatar.scale = avatar.scale;
            });
        }

        private void OnPlayerConnected(ExtendedPlayer player)
        {
            SendLocalAvatarPacket();
        }

        private void SendLocalAvatarPacket()
        {
            CustomAvatarPacket localAvatarPacket = localAvatar.GetPacket();
            Plugin.Log?.Info($"Sending 'CustomAvatarPacket' with {localAvatar.hash}");
            _sessionManager.Send(localAvatarPacket);
        }

        private void HandleAvatarPacket(CustomAvatarPacket packet, ExtendedPlayer player)
        {
            Plugin.Log?.Info($"Received 'CustomAvatarPacket' from '{player.userId}' with '{packet.hash}'");
            player.avatar = new CustomAvatarData(packet);
            avatarReceived?.Invoke(player);
        }
    }
}
