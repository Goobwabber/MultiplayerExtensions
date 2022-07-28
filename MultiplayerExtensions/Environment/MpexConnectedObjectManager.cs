using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environment
{
    public class MpexConnectedObjectManager : MonoBehaviour
    {
        private MultiplayerConnectedPlayerSpectatingSpot _playerSpectatingSpot = null!;
        private IConnectedPlayerBeatmapObjectEventManager _beatmapObjectEventManager = null!;
        private BeatmapObjectManager _beatmapObjectManager = null!;
        private Config _config = null!;

        [Inject]
        internal void Construct(
            MultiplayerConnectedPlayerSpectatingSpot playerSpectatingSpot,
            IConnectedPlayerBeatmapObjectEventManager beatmapObjectEventManager,
            BeatmapObjectManager beatmapObjectManager,
            Config config)
        {
            _playerSpectatingSpot = playerSpectatingSpot;
            _beatmapObjectEventManager = beatmapObjectEventManager;
            _beatmapObjectManager = beatmapObjectManager;
            _config = config;
        }

        private void Start()
        {
            _playerSpectatingSpot.isObservedChangedEvent += HandleIsObservedChangedEvent;
            if (_config.LagReducer)
                _beatmapObjectEventManager.Pause();
        }

        private void OnDestroy()
        {
            if (_playerSpectatingSpot != null)
                _playerSpectatingSpot.isObservedChangedEvent -= HandleIsObservedChangedEvent;
        }

        private void HandleIsObservedChangedEvent(bool isObserved)
        {
            if (_config.DisableMultiplayerPlatforms)
            {
                transform.Find("Lasers").gameObject.SetActive(isObserved);
                transform.Find("Construction").gameObject.SetActive(isObserved);
            }
            if (!_config.LagReducer)
                return;
            if (isObserved)
            {
                _beatmapObjectEventManager.Resume();
                return;
            }
            _beatmapObjectEventManager.Pause();
            _beatmapObjectManager.DissolveAllObjects();
        }
    }
}
