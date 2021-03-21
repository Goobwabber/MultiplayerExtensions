using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MultiplayerExtensions.OverrideClasses;
using Polyglot;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    class HostLobbySetupPanel : BSMLResourceViewController
    {
        public override string ResourceName => "MultiplayerExtensions.UI.HostLobbySetupPanel.bsml";
        private IMultiplayerSessionManager sessionManager;

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, HostLobbySetupViewController hostViewController, MultiplayerLevelLoader levelLoader)
        {
            this.sessionManager = sessionManager;
            base.DidActivate(true, false, true);

            hostViewController.didActivateEvent += OnActivate;
        }

        #region UIComponents
        [UIComponent("CustomSongsToggle")]
        public ToggleSetting customSongsToggle = null!;

        [UIComponent("FreeModToggle")]
        public ToggleSetting freeModToggle = null!;

        [UIComponent("VerticalHUDToggle")]
        public ToggleSetting verticalHUDToggle = null!;

        [UIComponent("DefaultHUDToggle")]
        public ToggleSetting defaultHUDToggle = null!;

        [UIComponent("HologramToggle")]
        public ToggleSetting hologramToggle = null!;

        [UIComponent("DownloadProgressText")]
        public FormattableText downloadProgressText = null!;
        #endregion

        #region UIValues
        [UIValue("CustomSongs")]
        public bool CustomSongs
        {
            get => Plugin.Config.CustomSongs;
            set { 
                Plugin.Config.CustomSongs = value;
                if (MPState.CustomSongsEnabled != value)
                {
                    MPState.CustomSongsEnabled = value;
                    MPEvents.RaiseCustomSongsChanged(this, value);
                }
            }
        }

        [UIValue("FreeMod")]
        public bool FreeMod
        {
            get => Plugin.Config.FreeMod;
            set { 
                Plugin.Config.FreeMod = value;
                if (MPState.FreeModEnabled != value)
                {
                    MPState.FreeModEnabled = value;
                    MPEvents.RaiseFreeModChanged(this, value);
                }
            }
        }

        [UIValue("VerticalHUD")]
        public bool VerticalHUD
        {
            get => Plugin.Config.VerticalHUD;
            set { Plugin.Config.VerticalHUD = value; }
        }

        [UIValue("DefaultHUD")]
        public bool DefaultHUD
        {
            get => Plugin.Config.SingleplayerHUD;
            set { Plugin.Config.SingleplayerHUD = value; }
        }

        [UIValue("Hologram")]
        public bool Hologram
        {
            get => Plugin.Config.Hologram;
            set { Plugin.Config.Hologram = value; }
        }

        [UIValue("DownloadProgress")]
        public string DownloadProgress
        {
            get => downloadProgressText.text;
            set { downloadProgressText.text = value; }
        }
        #endregion

        #region UIActions
        [UIAction("SetCustomSongs")]
        public void SetCustomSongs(bool value)
        {
            CustomSongs = value;
            customSongsToggle.Value = value;

            UpdateStates();
        }

        [UIAction("SetFreeMod")]
        public void SetFreeMod(bool value)
        {
            FreeMod = value;
            freeModToggle.Value = value;

            UpdateStates();
            SetModifierText();
        }

        [UIAction("SetVerticalHUD")]
        public void SetVerticalHUD(bool value)
        {
            VerticalHUD = value;
            verticalHUDToggle.Value = value;

            DefaultHUD = !(!DefaultHUD || !value);
            defaultHUDToggle.Value = !(!DefaultHUD || !value);
        }

        [UIAction("SetDefaultHUD")]
        public void SetDefaultHUD(bool value)
        {
            DefaultHUD = value;
            defaultHUDToggle.Value = value;

            VerticalHUD = VerticalHUD || value;
            verticalHUDToggle.Value = VerticalHUD || value;
        }

        [UIAction("SetHologram")]
        public void SetHologram(bool value)
        {
            Hologram = value;
            hologramToggle.Value = value;
        }
        #endregion

        private void OnActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                Transform spectatorText = transform.Find("Wrapper").Find("SpectatorModeWarningText");
                spectatorText.position = new Vector3(spectatorText.position.x, 0.25f, spectatorText.position.z);
            }
        }

        private void UpdateStates()
        {
            sessionManager?.SetLocalPlayerState("customsongs", CustomSongs);
            sessionManager?.SetLocalPlayerState("freemod", FreeMod);
        }

        private void SetModifierText()
        {
            var modifierTexts = Resources.FindObjectsOfTypeAll<CurvedTextMeshPro>().ToList().FindAll(text => text.gameObject.name == "SuggestedModifiers");
            foreach(CurvedTextMeshPro text in modifierTexts)
            {
                Destroy(text.gameObject.GetComponent<LocalizedTextMeshProUGUI>());
                text.text = MPState.FreeModEnabled ? "Selected Modifiers" : Localization.Get("SUGGESTED_MODIFIERS");
            }
        }
    }
}
