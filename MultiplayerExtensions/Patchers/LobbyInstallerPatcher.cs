using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Environments.Lobby;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;

namespace MultiplayerExtensions.Patchers
{
    public class LobbyInstallerPatcher : IAffinity
    {
        private readonly SiraLog _logger;

        internal LobbyInstallerPatcher(
            SiraLog logger)
        {
            _logger = logger;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLobbyInstaller), nameof(MultiplayerLobbyInstaller.InstallBindings))]
        private void InstallBindings(ref MultiplayerLobbyAvatarPlace ____multiplayerAvatarPlacePrefab, ref MultiplayerLobbyAvatarController ____multiplayerLobbyAvatarControllerPrefab)
        {
            _logger.Info("aaaaaaaaaaaaaaaaaaa");
            RedecorateAvatarPlace(ref ____multiplayerAvatarPlacePrefab);
        }

        private void RedecorateAvatarPlace(ref MultiplayerLobbyAvatarPlace originalPrefab)
        {
            if (originalPrefab.transform.parent != null && originalPrefab.transform.parent.name == "MultiplayerDecorator")
                return;

            GameObject mdgo = new("MultiplayerDecorator");
            mdgo.SetActive(false);
            var prefab = Object.Instantiate(originalPrefab, mdgo.transform);

            prefab.gameObject.AddComponent<MpexAvatarPlaceLighting>();

            originalPrefab = prefab;
        }

        private void RedecorateAvatar(ref MultiplayerLobbyAvatarController originalPrefab)
        {
            if (originalPrefab.transform.parent != null && originalPrefab.transform.parent.name == "MultiplayerDecorator")
                return;

            GameObject mdgo = new("MultiplayerDecorator");
            mdgo.SetActive(false);
            var prefab = Object.Instantiate(originalPrefab, mdgo.transform);

            var avatarCaption = prefab.transform.Find("AvatarCaption").gameObject;
            avatarCaption.AddComponent<MpexAvatarNameTag>();

            originalPrefab = prefab;
        }
    }
}
