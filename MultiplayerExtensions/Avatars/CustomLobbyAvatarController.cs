using CustomAvatar.Avatar;
using MultiplayerExtensions.Networking;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Avatars
{
    class CustomLobbyAvatarController : MonoBehaviour
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
            poseController = gameObject.GetComponentsInChildren<AvatarPoseController>().First();

            ExtendedPlayer extendedPlayer = _sessionManager.GetExtendedPlayer(_connectedPlayer);
            OnAvatarReceived(extendedPlayer);
        }

        private void OnAvatarReceived(ExtendedPlayer player)
        {
            avatarData = player.avatar;
            if (avatarData.hash == new CustomAvatarData().hash)
                return;

            _avatarProvider.FetchAvatarByHash(avatarData.hash, CancellationToken.None).ContinueWith(a =>
            {
                if (!a.IsFaulted && a.Result is LoadedAvatar)
                {
                    CreateAvatar(a.Result);
                }
            });
        }

        private void CreateAvatar(LoadedAvatar avatar)
        {
            loadedAvatar = avatar;
            if (spawnedAvatar != null)
                UnityEngine.Object.Destroy(spawnedAvatar);

            _avatarSpawner.SpawnAvatar(avatar, new MultiplayerInput(poseController), poseController.transform);
        }
    }
}
