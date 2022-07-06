using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Linq;

namespace MultiplayerExtensions.Patchers
{
    public class MenuEnvironmentPatcher : IAffinity
    {
        private readonly GameplaySetupViewController _gameplaySetup;
        private readonly Config _config;
        private readonly SiraLog _logger;

        internal MenuEnvironmentPatcher(
            GameplaySetupViewController gameplaySetup,
            Config config,
            SiraLog logger)
        {
            _gameplaySetup = gameplaySetup;
            _config = config;
            _logger = logger;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameplaySetupViewController), nameof(GameplaySetupViewController.Setup))]
        private void EnableEnvironmentTab(bool showModifiers, ref bool showEnvironmentOverrideSettings, bool showColorSchemesSettings, bool showMultiplayer, PlayerSettingsPanelController.PlayerSettingsPanelLayout playerSettingsPanelLayout)
        {
            if (showMultiplayer)
                showEnvironmentOverrideSettings = _config.SoloEnvironment;
        }

        private EnvironmentInfoSO _originalEnvironmentInfo = null!;

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
        private void SetEnvironmentScene(IDifficultyBeatmap difficultyBeatmap, ref EnvironmentInfoSO ____multiplayerEnvironmentInfo)
        {
            if (!_config.SoloEnvironment)
                return;

            _originalEnvironmentInfo = ____multiplayerEnvironmentInfo;
            ____multiplayerEnvironmentInfo = difficultyBeatmap.GetEnvironmentInfo();
            if (_gameplaySetup.environmentOverrideSettings.overrideEnvironments)
                ____multiplayerEnvironmentInfo = _gameplaySetup.environmentOverrideSettings.GetOverrideEnvironmentInfoForType(____multiplayerEnvironmentInfo.environmentType);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
        private void ResetEnvironmentScene(IDifficultyBeatmap difficultyBeatmap, ref EnvironmentInfoSO ____multiplayerEnvironmentInfo)
        {
            if (_config.SoloEnvironment)
                ____multiplayerEnvironmentInfo = _originalEnvironmentInfo;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ScenesTransitionSetupDataSO), "Init")]
        private void AddEnvironmentOverrides(ref SceneInfo[] scenes)
        {
            if (_config.SoloEnvironment && scenes.Any(scene => scene.name.Contains("Multiplayer")))
            {
                scenes = scenes.AddItem(_originalEnvironmentInfo.sceneInfo).ToArray();
            }
        }
    }
}
