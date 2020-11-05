using CustomAvatar.Avatar;
using MultiplayerExtensions.Networking;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Avatars
{
    class CustomAvatarController : MonoBehaviour
    {
        [Inject]
        private ExtendedSessionManager _sessionManager;

        [Inject]
        private CustomAvatarManager _customAvatarManager;

        [Inject]
        private AvatarSpawner _avatarSpawner;

        [Inject]
        private IAvatarProvider<LoadedAvatar> _avatarProvider;

        [InjectOptional]
        protected readonly IConnectedPlayer _connectedPlayer;

        private CustomAvatarData avatarData;
        private LoadedAvatar? loadedAvatar;
        private SpawnedAvatar? spawnedAvatar;
        private AvatarPoseController poseController;

        public virtual void Start()
        {
            _customAvatarManager.avatarReceived += OnAvatarReceived;

            TryGetPoseController();
        }

        public virtual void Update()
        {
            if (poseController == null)
            {
                TryGetPoseController();
            }
        }

        public void TryGetPoseController()
        {
            var poseControllers = gameObject.GetComponentsInChildren<AvatarPoseController>();
            if(poseControllers.Length != 0)
            {
                poseController = poseControllers.First();
                ExtendedPlayer extendedPlayer = _sessionManager.GetExtendedPlayer(_connectedPlayer);
                OnAvatarReceived(extendedPlayer);
            }
        }

        private void OnAvatarReceived(ExtendedPlayer player)
        {
            if (player.avatar == null)
                return;

            if (player.avatar.hash == new CustomAvatarData().hash)
                return;

            avatarData = player.avatar;

            _avatarProvider.FetchAvatarByHash(avatarData.hash, CancellationToken.None).ContinueWith(a =>
            {
                if (!a.IsFaulted && a.Result is LoadedAvatar)
                {
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        CreateAvatar(a.Result);
                    });
                }
            });
        }

        private void CreateAvatar(LoadedAvatar avatar)
        {
            loadedAvatar = avatar;
            if (spawnedAvatar != null)
                UnityEngine.Object.Destroy(spawnedAvatar);

            poseController.gameObject.SetActive(false);
            spawnedAvatar = _avatarSpawner.SpawnAvatar(avatar, new MultiplayerAvatarInput(poseController), transform);
            spawnedAvatar.SetLocomotionEnabled(true);
            spawnedAvatar.scale = avatarData.scale;
        }
    }
}
