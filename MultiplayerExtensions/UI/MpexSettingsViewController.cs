using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
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


        [UIValue("lag-reducer")]
        private bool _lagReducer
        {
            get => _config.LagReducer;
            set
            {
                _config.LagReducer = value;
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
                NotifyPropertyChanged();
            }
        }
    }
}
