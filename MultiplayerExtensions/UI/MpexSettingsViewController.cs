using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;

namespace MultiplayerExtensions.UI
{
    [ViewDefinition("MultiplayerExtensions.UI.MpexSettingsViewController.bsml")]
    public class MpexSettingsViewController : BSMLAutomaticViewController
    {
        private Config _config = null!;

        [Inject]
        private void Construct(
            Config config)
        {
            _config = config;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _personalMissLightingToggle.interactable = _missLighting;
        }

        [UIComponent("personal-miss-lighting-toggle")]
        private GenericInteractableSetting _personalMissLightingToggle = null!;

        [UIValue("hide-player-platforms")]
        private bool _hidePlayerPlatforms
        {
            get => _config.DisableMultiplayerPlatforms;
            set
            {
                _config.DisableMultiplayerPlatforms = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("hide-player-lights")]
        private bool _hidePlayerLights
        {
            get => _config.DisableMultiplayerLights;
            set
            {
                _config.DisableMultiplayerLights = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("hide-player-objects")]
        private bool _hidePlayerObjects
        {
            get => _config.DisableMultiplayerObjects;
            set
            {
                _config.DisableMultiplayerObjects = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("miss-lighting")]
        private bool _missLighting
        {
            get => _config.MissLighting;
            set
            {
                _config.MissLighting = value;
                if (_personalMissLightingToggle != null)
                    _personalMissLightingToggle.interactable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("personal-miss-lighting-only")]
        private bool _personalMissLightingOnly
        {
            get => _config.PersonalMissLightingOnly;
            set
            {
                _config.PersonalMissLightingOnly = value;
                NotifyPropertyChanged();
            }
        }
    }
}
