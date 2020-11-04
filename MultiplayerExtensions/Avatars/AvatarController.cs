using BS_Utils.Utilities;
using CustomAvatar.Avatar;
using CustomAvatar.Player;
using MultiplayerExtensions.Avatars;
using MultiplayerExtensions.Downloaders;
using System;
using System.Collections.Generic;
using System.Threading;
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
        private IAvatarProvider<LoadedAvatar> _avatarProvider;

        private Transform _container;

        private readonly Dictionary<string, MultiplayerAvatar> playerPoseControllers = new Dictionary<string, MultiplayerAvatar>();

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
        internal AvatarController(CustomMultiplayerController multiplayerController, AvatarSpawner avatarSpawner, PlayerAvatarManager playerAvatarManager, VRPlayerInput playerInput, FloorController floorController, IAvatarProvider<LoadedAvatar> avatarProvider)
        {
            _multiplayerController = multiplayerController;
            _avatarSpawner = avatarSpawner;
            _avatarManager = playerAvatarManager;
            _playerInput = playerInput;
            _floorController = floorController;
            _avatarProvider = avatarProvider;
            _container = null!;
            if (_avatarProvider == null)
                Plugin.Log?.Warn("_avatarProvider is null!");
            else
                Plugin.Log?.Info("_avatarProvider is not null!");
        }

        private void HandleLocalAvatarUpdate()
        {
            localAvatar.pelvis = _playerInput.allowMaintainPelvisPosition;
            _multiplayerController.Send(localAvatar.GetAvatarPacket());
        }

        private void HandlePlayerAvatarUpdate(CustomAvatarPacket packet, IConnectedPlayer player)
        {
            _multiplayerController.players[player.userId].customAvatar = new CustomAvatarData(packet);
            playerPoseControllers[player.userId] = new MultiplayerAvatar(this, _multiplayerController.players[player.userId], _avatarProvider);
        }

        private void OnAvatarChanged(SpawnedAvatar avatar)
        {
            localAvatar.hash ??= "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
            if (!avatar) return;

            _avatarProvider.HashAvatar(avatar.avatar).ContinueWith(r =>
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
            public LoadedAvatar? loadedAvatar { get; private set; }
            public CustomMultiplayerController.CustomPlayer player { get; private set; }
            public CustomAvatarData? avatarData { get; private set; }

            private MultiplayerAvatarPoseController _multiplayerPoseController;
            private AvatarPoseController _poseController;
            private SpawnedAvatar? _spawnedAvatar;
            private readonly IAvatarProvider<LoadedAvatar> _avatarProvider;

            public MultiplayerAvatar(AvatarController avatarController, CustomMultiplayerController.CustomPlayer player, IAvatarProvider<LoadedAvatar> avatarProvider)
            {
                this.avatarController = avatarController;
                this.player = player;
                this.avatarData = player.customAvatar;
                this._avatarProvider = avatarProvider;

                _multiplayerPoseController = Array.Find(Resources.FindObjectsOfTypeAll<MultiplayerAvatarPoseController>(), x => x.GetField<IConnectedPlayer>("_connectedPlayer").userId == player.userId);
                _poseController = _multiplayerPoseController.GetField<AvatarPoseController>("_avatarPoseController");

                //_avatarProvider.avatarDownloaded += OnAvatarDownloaded;

                if (_avatarProvider.TryGetCachedAvatar(player.customAvatar.hash, out LoadedAvatar? avatar))
                {
                    loadedAvatar = avatar;
                    CreateAvatar();
                }
                else
                {
                    _avatarProvider.FetchAvatarInfoByHash(player.customAvatar.hash, CancellationToken.None).ContinueWith(async i =>
                    {
                        if(!i.IsFaulted && i.Result is AvatarInfo avatarInfo)
                        {
                            var path = await avatarInfo.DownloadAvatar(CancellationToken.None);
                            if (path != null)
                            {
                                LoadedAvatar? avatar = await _avatarProvider.LoadAvatar(path);
                                if (avatar != null)
                                {
                                    loadedAvatar = avatar;
                                    CreateAvatar();
                                }
                            }
                        }
                    });
                }
            }

            private void OnAvatarDownloaded(object sender, AvatarDownloadedEventArgs e)
            {
                if (e.Hash == player.customAvatar.hash && sender is IAvatarProvider<LoadedAvatar> provider)
                {
                    if (provider.TryGetCachedAvatar(e.Hash, out LoadedAvatar cachedAvatar))
                    {
                        loadedAvatar = cachedAvatar;
                        CreateAvatar();
                    }
                }
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
