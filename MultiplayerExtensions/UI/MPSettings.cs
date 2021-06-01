using BeatSaberMarkupLanguage.Attributes;

namespace MultiplayerExtensions.UI
{
    public class MPSettings : PersistentSingleton<MPSettings>
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
