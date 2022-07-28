using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;

namespace MultiplayerExtensions.UI
{
    [ViewDefinition("MultiplayerExtensions.UI.MpexMiscViewController.bsml")]
    public class MpexMiscViewController : BSMLAutomaticViewController
    {
        private Config _config = null!;

        [Inject]
        private void Construct(
            Config config)
        {
            _config = config;
        }

        [UIValue("disable-avatar-constraints")]
        private bool _disableAvatarConstraints
        {
            get => _config.DisableAvatarConstraints;
            set
            {
                _config.DisableAvatarConstraints = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("disable-player-colors")]
        private bool _disablePlayerColors
        {
            get => _config.DisableMultiplayerColors;
            set
            {
                _config.DisableMultiplayerColors = value;
                NotifyPropertyChanged();
            }
        }
    }
}
