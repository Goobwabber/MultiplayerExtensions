using BS_Utils.Utilities;
using CustomAvatar.Avatar;
using CustomAvatar.Player;
using MultiplayerExtensions.Avatars;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Avatars
{
    internal class AvatarController : IInitializable, IDisposable
    {
        private CustomMultiplayerController _multiplayerController;
        private AvatarSpawner _avatarSpawner;
        private PlayerAvatarManager _avatarManager;
        private VRPlayerInput _playerInput;
        private FloorController _floorController;

        private Transform _container;

        private Dictionary<string, MultiplayerAvatar> playerPoseControllers = new Dictionary<string, MultiplayerAvatar>();

        public CustomAvatarData localAvatar = new CustomAvatarData();

        public PlayerAvatarManager avatarManager { get { return _avatarManager; } }

        public void Initialize()
        {
            Plugin.Log?.Info("Avatar Controller created.");
            _container = new GameObject(nameof(AvatarController)).transform;
            UnityEngine.Object.DontDestroyOnLoad(_container);

            _multiplayerController.connectedEvent += HandleLocalAvatarUpdate;
            _multiplayerController.RegisterCallback<CustomAvatarPacket>(CustomMultiplayerController.MessageType.AvatarUpdate, HandlePlayerAvatarUpdate, new Func<CustomAvatarPacket>(CustomAvatarPacket.pool.Obtain));

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
        internal void Inject(CustomMultiplayerController multiplayerController, AvatarSpawner avatarSpawner, PlayerAvatarManager playerAvatarManager, VRPlayerInput playerInput, FloorController floorController)
        {
            _multiplayerController = multiplayerController;
            _avatarSpawner = avatarSpawner;
            _avatarManager = playerAvatarManager;
            _playerInput = playerInput;
            _floorController = floorController;
        }

        private void HandleLocalAvatarUpdate()
        {
            localAvatar.pelvis = _playerInput.allowMaintainPelvisPosition;
            _multiplayerController.Send(localAvatar.GetAvatarPacket());
        }

        private void HandlePlayerAvatarUpdate(CustomAvatarPacket packet, IConnectedPlayer player)
        {
            _multiplayerController.players[player.userId].customAvatar = new CustomAvatarData(packet);
            playerPoseControllers[player.userId] = new MultiplayerAvatar(this, _multiplayerController.players[player.userId]);
        }

        private void OnAvatarChanged(SpawnedAvatar avatar)
        {
            localAvatar.hash ??= "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
            if (!avatar) return;

            ModelSaber.HashAvatar(avatar.avatar).ContinueWith(r =>
            {
                localAvatar.hash = r.Result;
                HandleLocalAvatarUpdate();
            });
        }

        private void OnAvatarScaleChanged(float scale)
        {
            localAvatar.scale = scale;
            HandleLocalAvatarUpdate();
        }

        private void OnFloorPositionChanged(float verticalPosition)
        {
            localAvatar.floor = verticalPosition;
            HandleLocalAvatarUpdate();
        }

        class MultiplayerAvatar
        {
            public AvatarController avatarController { get; private set; }
            public LoadedAvatar loadedAvatar { get; private set; }
            public CustomMultiplayerController.CustomPlayer player { get; private set; }
            public CustomAvatarData avatarData { get; private set; }

            private MultiplayerAvatarPoseController _multiplayerPoseController;
            private AvatarPoseController _poseController;
            private SpawnedAvatar _spawnedAvatar;

            public MultiplayerAvatar(AvatarController avatarController, CustomMultiplayerController.CustomPlayer player)
            {
                this.avatarController = avatarController;
                this.player = player;
                this.avatarData = player.customAvatar;

                loadedAvatar = ModelSaber.cachedAvatars[player.customAvatar.hash];

                _multiplayerPoseController = Array.Find(Resources.FindObjectsOfTypeAll<MultiplayerAvatarPoseController>(), x => x.GetField<IConnectedPlayer>("_connectedPlayer").userId == player.userId);
                _poseController = _multiplayerPoseController.GetField<AvatarPoseController>("_avatarPoseController");

                CreateAvatar();
            }

            public void CreateAvatar()
            {
                DestroyAvatar();

                _spawnedAvatar = avatarController._avatarSpawner.SpawnAvatar(loadedAvatar, new MultiplayerInput(_poseController), _poseController.transform);
                _spawnedAvatar.scale = avatarData.scale;
            }

            public void UpdateAvatar(LoadedAvatar avatar)
            {
                loadedAvatar = avatar;
                CreateAvatar();
            }

            public void DestroyAvatar()
            {
                if (_spawnedAvatar != null)
                    UnityEngine.Object.Destroy(_spawnedAvatar);
            }
        }
    }
}
