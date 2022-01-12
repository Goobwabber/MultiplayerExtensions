using IPA.Utilities;
using MultiplayerExtensions.Environment;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MpexGameInstaller : Installer
    {
        private readonly FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>.Accessor _activeLocalPlayerControllerPrefab 
            = FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>
                .GetAccessor(nameof(_activeLocalPlayerControllerPrefab));

        private readonly FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>.Accessor _activeLocalPlayerDuelControllerPrefab 
            = FieldAccessor<MultiplayerPlayersManager, MultiplayerLocalActivePlayerFacade>
                .GetAccessor(nameof(_activeLocalPlayerDuelControllerPrefab));

        private readonly FieldAccessor<MultiplayerPlayersManager, MultiplayerConnectedPlayerFacade>.Accessor _connectedPlayerControllerPrefab
            = FieldAccessor<MultiplayerPlayersManager, MultiplayerConnectedPlayerFacade>
                .GetAccessor(nameof(_connectedPlayerControllerPrefab));

        private readonly FieldAccessor<MultiplayerPlayersManager, MultiplayerConnectedPlayerFacade>.Accessor _connectedPlayerDuelControllerPrefab
            = FieldAccessor<MultiplayerPlayersManager, MultiplayerConnectedPlayerFacade>
                .GetAccessor(nameof(_connectedPlayerDuelControllerPrefab));

        public override void InstallBindings()
        {
            var playersManager = Container.Resolve<MultiplayerPlayersManager>();
            RedecoratePlayerFacade(ref _activeLocalPlayerControllerPrefab(ref playersManager));
            RedecoratePlayerFacade(ref _activeLocalPlayerDuelControllerPrefab(ref playersManager));
            RedecoratePlayerFacade(ref _connectedPlayerControllerPrefab(ref playersManager));
            RedecoratePlayerFacade(ref _connectedPlayerDuelControllerPrefab(ref playersManager));
        }

        private void RedecoratePlayerFacade<TPrefab>(ref TPrefab originalPrefab) where TPrefab : MonoBehaviour
        {
            if (originalPrefab.transform.parent != null && originalPrefab.transform.parent.name == "MultiplayerDecorator")
                return;

            GameObject mdgo = new("MultiplayerDecorator");
            mdgo.SetActive(false);
            var prefab = Object.Instantiate(originalPrefab, mdgo.transform);

            var gameplayAnimator = prefab.GetComponentInChildren<MultiplayerGameplayAnimator>();
            gameplayAnimator.gameObject.AddComponent<MpexPlayerFacadeLighting>();

            originalPrefab = prefab;
        }
    }
}
