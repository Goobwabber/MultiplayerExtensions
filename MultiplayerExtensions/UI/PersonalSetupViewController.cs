using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.UI
{
    public class PersonalSetupViewController : BSMLResourceViewController
    {
        public override string ResourceName => "MultiplayerExtensions.UI.PersonalSetupView.bsml";

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
