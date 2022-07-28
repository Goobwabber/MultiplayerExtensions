using SiraUtil.Affinity;

namespace MultiplayerExtensions.Patchers
{
    public class ColorSchemePatcher : IAffinity
    {
        private readonly GameplayCoreSceneSetupData _sceneSetupData;
        private readonly Config _config;

        internal ColorSchemePatcher(
            GameplayCoreSceneSetupData sceneSetupData,
            Config config)
        {
            _sceneSetupData = sceneSetupData;
            _config = config;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(PlayersSpecificSettingsAtGameStartModel), nameof(PlayersSpecificSettingsAtGameStartModel.GetPlayerSpecificSettingsForUserId))]
        private void SetConnectedPlayerColorScheme(ref PlayerSpecificSettingsNetSerializable __result)
        {
            var colorscheme = _sceneSetupData.colorScheme;
            if (_config.DisableMultiplayerColors)
                __result.colorScheme = new ColorSchemeNetSerializable(colorscheme.saberAColor, colorscheme.saberBColor, colorscheme.obstaclesColor, colorscheme.environmentColor0, colorscheme.environmentColor1, colorscheme.environmentColor0Boost, colorscheme.environmentColor1Boost);
        }
    }
}
