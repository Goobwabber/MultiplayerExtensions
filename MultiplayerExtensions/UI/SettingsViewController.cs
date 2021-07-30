using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using Zenject;

namespace MultiplayerExtensions.UI
{
    [HotReload(RelativePathToLayout = @"..\UI\SettingsView.bsml")]
    [ViewDefinition("MultiplayerExtensions.UI.SettingsView.bsml")]
    public class SettingsViewController : BSMLAutomaticViewController
    {

        [UIValue("eastereggs")]
        public bool EasterEggsEnabled
        {
            get => MPState.EasterEggsEnabled;
            set
            {
                MPState.EasterEggsEnabled = value;
            }
        }

        [UIValue("statistics")]
        public bool StatisticsEnabled
        {
            get => Plugin.Config.Statistics;
            set
            {
                Plugin.Config.Statistics = value;
            }
        }
    }
}
