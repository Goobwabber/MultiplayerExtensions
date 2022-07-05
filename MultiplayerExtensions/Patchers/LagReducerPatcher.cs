using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace MultiplayerExtensions.Patchers
{
    public class LagReducerPatcher : IAffinity
    {
        private FieldAccessor<MultiplayerConnectedPlayerBeatmapObjectEventManager, IConnectedPlayer>.Accessor _eventConnectedPlayer
            = FieldAccessor<MultiplayerConnectedPlayerBeatmapObjectEventManager, IConnectedPlayer>.GetAccessor("_connectedPlayer");
        private FieldAccessor<MultiplayerConnectedPlayerSpectatingSpot, IConnectedPlayer>.Accessor _spotConnectedPlayer
            = FieldAccessor<MultiplayerConnectedPlayerSpectatingSpot, IConnectedPlayer>.GetAccessor("_connectedPlayer");

        private readonly MultiplayerSpectatorController? _spectatorController;
        private readonly Config _config;
        private readonly SiraLog _logger;

        internal LagReducerPatcher(
            [InjectOptional]MultiplayerSpectatorController spectatorController,
            Config config,
            SiraLog logger)
        {
            _spectatorController = spectatorController;
            _config = config;
            _logger = logger;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerConnectedPlayerBeatmapObjectManager), nameof(MultiplayerConnectedPlayerBeatmapObjectManager.ProcessNoteData))]
        private bool ProcessNoteDataPatch(ref IConnectedPlayerBeatmapObjectEventManager ____beatmapObjectEventManager)
        {
            if (!_config.LagReducer)
                return true;
            if (_spectatorController == null)
                return false;
            var beatmapObjectEventManager = (MultiplayerConnectedPlayerBeatmapObjectEventManager)____beatmapObjectEventManager;
            if (_spectatorController.currentSpot is MultiplayerConnectedPlayerSpectatingSpot playerSpectatingSpot)
                return _spotConnectedPlayer(ref playerSpectatingSpot) == _eventConnectedPlayer(ref beatmapObjectEventManager);
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerConnectedPlayerBeatmapObjectManager), nameof(MultiplayerConnectedPlayerBeatmapObjectManager.ProcessObstacleData))]
        private bool ProcessObstacleDataPatch(ref IConnectedPlayerBeatmapObjectEventManager ____beatmapObjectEventManager)
        {
            if (!_config.LagReducer)
                return true;
            if (_spectatorController == null)
                return false;
            var beatmapObjectEventManager = (MultiplayerConnectedPlayerBeatmapObjectEventManager)____beatmapObjectEventManager;
            if (_spectatorController.currentSpot is MultiplayerConnectedPlayerSpectatingSpot playerSpectatingSpot)
                return _spotConnectedPlayer(ref playerSpectatingSpot) == _eventConnectedPlayer(ref beatmapObjectEventManager);
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerConnectedPlayerBeatmapObjectManager), nameof(MultiplayerConnectedPlayerBeatmapObjectManager.ProcessSliderData))]
        private bool ProcessSliderDataPatch(ref IConnectedPlayerBeatmapObjectEventManager ____beatmapObjectEventManager)
        {
            if (!_config.LagReducer)
                return true;
            if (_spectatorController == null)
                return false;
            var beatmapObjectEventManager = (MultiplayerConnectedPlayerBeatmapObjectEventManager)____beatmapObjectEventManager;
            if (_spectatorController.currentSpot is MultiplayerConnectedPlayerSpectatingSpot playerSpectatingSpot)
                return _spotConnectedPlayer(ref playerSpectatingSpot) == _eventConnectedPlayer(ref beatmapObjectEventManager);
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerConnectedPlayerEffectsSpawner), nameof(MultiplayerConnectedPlayerEffectsSpawner.HandleBeatmapObjectEventManagerConnectedPlayerBeatmapObjectWasCut))]
        private bool HandleNoteCutPatch(ref IConnectedPlayerBeatmapObjectEventManager ____beatmapObjectEventManager)
        {
            if (!_config.LagReducer)
                return true;
            if (_spectatorController == null)
                return false;
            var beatmapObjectEventManager = (MultiplayerConnectedPlayerBeatmapObjectEventManager)____beatmapObjectEventManager;
            if (_spectatorController.currentSpot is MultiplayerConnectedPlayerSpectatingSpot playerSpectatingSpot)
                return _spotConnectedPlayer(ref playerSpectatingSpot) == _eventConnectedPlayer(ref beatmapObjectEventManager);
            return false;
        }
    }
}
