using CustomAvatar.Avatar;
using CustomAvatar.Player;
using MultiplayerExtensions.Avatars;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Controllers
{
    internal class AvatarController : IInitializable, IDisposable
    {
        private CustomMultiplayerController _multiplayerController;
        private PlayerAvatarManager _avatarManager;
        private VRPlayerInput _playerInput;
        private FloorController _floorController;

        private Transform _container;

        public string avatarHash { get; private set; }
        public float avatarScale { get; private set; }
        public float avatarFloor { get; private set; }

        public PlayerAvatarManager avatarManager { get { return _avatarManager; } }

        public void Initialize()
        {
            Plugin.Log?.Info("Avatar Controller created.");
            _container = new GameObject(nameof(AvatarController)).transform;
            UnityEngine.Object.DontDestroyOnLoad(_container);

            _multiplayerController.connectedEvent += HandleLocalAvatarUpdate;
            _multiplayerController.RegisterCallback<AvatarPacket>(CustomMultiplayerController.MessageType.AvatarUpdate, HandlePlayerAvatarUpdate, new Func<AvatarPacket>(AvatarPacket.pool.Obtain));

            _avatarManager.avatarChanged += OnAvatarChanged;
            _avatarManager.avatarScaleChanged += OnAvatarScaleChanged;
            _floorController.floorPositionChanged += OnFloorPositionChanged;
            OnAvatarChanged(_avatarManager.currentlySpawnedAvatar);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_container);

            _avatarManager.avatarChanged -= OnAvatarChanged;
            _avatarManager.avatarScaleChanged -= OnAvatarScaleChanged;
            _floorController.floorPositionChanged -= OnFloorPositionChanged;
        }
        
        [Inject]
        internal void Inject(CustomMultiplayerController multiplayerController, PlayerAvatarManager playerAvatarManager, VRPlayerInput playerInput, FloorController floorController)
        {
            _multiplayerController = multiplayerController;
            _avatarManager = playerAvatarManager;
            _playerInput = playerInput;
            _floorController = floorController;
        }

        private void HandleLocalAvatarUpdate()
        {
            _multiplayerController.Send<AvatarPacket>(new AvatarPacket().Init(avatarHash, avatarScale, avatarFloor));
        }

        private void HandlePlayerAvatarUpdate(AvatarPacket packet, IConnectedPlayer player)
        {
            Plugin.Log?.Info($"{player.userName} avatar: {packet.avatarHash}");
        }

        private void OnAvatarChanged(SpawnedAvatar avatar)
        {
            avatarHash ??= "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
            if (!avatar) return;

            ModelSaber.HashAvatar(avatar.avatar).ContinueWith(r =>
            {
                avatarHash = r.Result;
                HandleLocalAvatarUpdate();
            });
        }

        private void OnAvatarScaleChanged(float scale)
        {
            avatarScale = scale;
            HandleLocalAvatarUpdate();
        }

        private void OnFloorPositionChanged(float verticalPosition)
        {
            avatarFloor = verticalPosition;
            HandleLocalAvatarUpdate();
        }
    }
}
