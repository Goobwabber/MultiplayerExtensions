using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using IPA.Utilities;
using Zenject;

namespace MultiplayerExtensions.UI
{
    [ViewDefinition("MultiplayerExtensions.UI.MpexEnvironmentViewController.bsml")]
    public class MpexEnvironmentViewController : BSMLAutomaticViewController
    {
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showModifiers
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showModifiers));
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showEnvironmentOverrideSettings
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showEnvironmentOverrideSettings));
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showColorSchemesSettings
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showColorSchemesSettings));
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showMultiplayer
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showMultiplayer));

        private GameplaySetupViewController _gameplaySetup = null!;
        private Config _config = null!;

        [Inject]
        private void Construct(
            GameplaySetupViewController gameplaySetup,
            Config config)
        {
            _gameplaySetup = gameplaySetup;
            _config = config;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _sideBySideDistanceIncrement.interactable = _sideBySide;
        }

        [UIComponent("side-by-side-distance-increment")]
        private GenericInteractableSetting _sideBySideDistanceIncrement = null!;

        [UIValue("solo-environment")]
        private bool _soloEnvironment
        {
            get => _config.SoloEnvironment;
            set
            {
                _config.SoloEnvironment = value;
                _gameplaySetup.Setup(
                    _showModifiers(ref _gameplaySetup),
                    _showEnvironmentOverrideSettings(ref _gameplaySetup),
                    _showColorSchemesSettings(ref _gameplaySetup),
                    _showMultiplayer(ref _gameplaySetup),
                    PlayerSettingsPanelController.PlayerSettingsPanelLayout.Multiplayer
                );
                NotifyPropertyChanged();
            }
        }

        [UIValue("side-by-side")]
        private bool _sideBySide
        {
            get => _config.SideBySide;
            set
            {
                _config.SideBySide = value;
                if (_sideBySideDistanceIncrement != null)
                    _sideBySideDistanceIncrement.interactable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("side-by-side-distance")]
        private float _sideBySideDistance
        {
            get => _config.SideBySideDistance;
            set
            {
                _config.SideBySideDistance = value;
                NotifyPropertyChanged();
            }
        }
    }
}
