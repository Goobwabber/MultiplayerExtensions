using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;

namespace MultiplayerExtensions.UI
{
    [HotReload(RelativePathToLayout = @"..\UI\OpenSettingsView.bsml")]
    [ViewDefinition("MultiplayerExtensions.UI.OpenSettingsView.bsml")]
    public class OpenSettingsViewController : BSMLAutomaticViewController
    {
        internal event Action OpenSettingsButtonClicked;

        [UIAction("open-settings-clicked")] 
        private void OSC() => OpenSettingsButtonClicked?.Invoke();
    }
}
