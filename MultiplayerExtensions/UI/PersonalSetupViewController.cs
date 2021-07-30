using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    [HotReload(RelativePathToLayout = @"..\UI\PersonalSetupView.bsml")]
    [ViewDefinition("MultiplayerExtensions.UI.PersonalSetupView.bsml")]
    public class PersonalSetupViewController : BSMLAutomaticViewController
    {
        private MultiplayerSettingsPanelController multiplayerSettingsPanelController = null!;

        [UIComponent("toggle")]
        private readonly RectTransform toggleTransform = null!;

        [Inject]
        public void Construct(GameplaySetupViewController gameplaySetupViewController)
        {
            multiplayerSettingsPanelController = gameplaySetupViewController.GetField<MultiplayerSettingsPanelController, GameplaySetupViewController>("_multiplayerSettingsPanelController");
        }

        [UIAction("#post-parse")]
        public void PostParse()
        {
            Transform spectatorTransform = multiplayerSettingsPanelController.transform.Find("Spectator");
            spectatorTransform.SetParent(toggleTransform.parent);
            spectatorTransform.gameObject.SetActive(true);
            spectatorTransform.SetAsFirstSibling();
        }

        [UIValue("singleplayer-hud")]
        private bool SingleplayerHUD
        {
            get => Plugin.Config.SingleplayerHUD;
            set
            {
                Plugin.Config.SingleplayerHUD = value;
                NotifyPropertyChanged(nameof(SingleplayerHUD));
                Plugin.Config.VerticalHUD = VerticalHUD || value;
                NotifyPropertyChanged(nameof(VerticalHUD));
            }
        }

        [UIValue("vertical-hud")]
        private bool VerticalHUD
        {
            get => Plugin.Config.VerticalHUD;
            set
            {
                Plugin.Config.VerticalHUD = value;
                NotifyPropertyChanged(nameof(VerticalHUD));
                Plugin.Config.SingleplayerHUD = !(!SingleplayerHUD || !value);
                NotifyPropertyChanged(nameof(SingleplayerHUD));
            }
        }


        [UIValue("hologram")]
        private bool Hologram
        {
            get => Plugin.Config.Hologram;
            set
            {
                Plugin.Config.Hologram = value;
                NotifyPropertyChanged(nameof(Hologram));
            }
        }

        [UIValue("lag-reducer")]
        private bool LagReducer
        {
            get => Plugin.Config.LagReducer;
            set
            {
                Plugin.Config.LagReducer = value;
                NotifyPropertyChanged(nameof(LagReducer));
            }
        }

        [UIValue("miss-lighting")]
        private bool MissLighting
        {
            get => Plugin.Config.MissLighting;
            set
            {
                Plugin.Config.MissLighting = value;
                NotifyPropertyChanged(nameof(MissLighting));
            }
        }
    }
}
