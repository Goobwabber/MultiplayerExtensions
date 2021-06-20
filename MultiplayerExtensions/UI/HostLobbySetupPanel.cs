using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MultiplayerExtensions.OverrideClasses;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    class HostLobbySetupPanel : BSMLResourceViewController, IInitializable, IDisposable
    {
        public override string ResourceName => "MultiplayerExtensions.UI.LobbySetupPanel.bsml";
        private IMultiplayerSessionManager sessionManager = null!;
        private HostLobbySetupViewController hostViewController = null!;
        private EmotePanel emotePanel = null!;

        CurvedTextMeshPro? modifierText;

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, HostLobbySetupViewController hostViewController, MultiplayerLevelLoader levelLoader, EmotePanel emotePanel)
        {
            this.sessionManager = sessionManager;
            this.hostViewController = hostViewController;
            this.emotePanel = emotePanel;
            base.DidActivate(true, false, true);
        }

        public void Initialize()
        {
            hostViewController.didActivateEvent += OnActivate;
        }

        public void Dispose()
        {
            hostViewController.didActivateEvent -= OnActivate;
        }

        private void OnActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                Transform spectatorText = transform.Find("Wrapper").Find("SpectatorModeWarningText");
                spectatorText.position = new Vector3(spectatorText.position.x, 0.25f, spectatorText.position.z);
            }
        }

        public void OnDisable()
        {
            emotePanel.CloseScreen();
        }

        private void UpdateStates()
        {
            sessionManager?.SetLocalPlayerState("customsongs", Plugin.Config.CustomSongs);
            sessionManager?.SetLocalPlayerState("freemod", Plugin.Config.FreeMod);
            sessionManager?.SetLocalPlayerState("hostpick", Plugin.Config.HostPick);
        }

        private void SetModifierText()
        {
            if (modifierText == null)
            {
                modifierText = Resources.FindObjectsOfTypeAll<CurvedTextMeshPro>().ToList().Find(text => text.gameObject.name == "SuggestedModifiers");
                Destroy(modifierText.gameObject.GetComponent<LocalizedTextMeshPro>());
            }

            if (modifierText != null)
                modifierText.text = MPState.FreeModEnabled ? "Selected Modifiers" : Localization.Get("SUGGESTED_MODIFIERS");
        }

        #region UIComponents
        [UIComponent("custom-songs-toggle")]
        public ToggleSetting customSongsToggle = null!;

        [UIComponent("freemod-toggle")]
        public ToggleSetting freeModToggle = null!;

        [UIComponent("host-pick-toggle")]
        public ToggleSetting hostPickToggle = null!;

        [UIComponent("vertical-hud-toggle")]
        public ToggleSetting verticalHUDToggle = null!;

        [UIComponent("default-hud-toggle")]
        public ToggleSetting defaultHUDToggle = null!;

        [UIComponent("hologram-toggle")]
        public ToggleSetting hologramToggle = null!;

        [UIComponent("lag-reducer-toggle")]
        public ToggleSetting lagReducerToggle = null!;

        [UIComponent("miss-lighting-toggle")]
        public ToggleSetting missLightingToggle = null!;
        #endregion

        #region UIActions
        [UIAction("set-custom-songs")]
        public void SetCustomSongs(bool value)
        {
            Plugin.Config.CustomSongs = value;
            if (MPState.CustomSongsEnabled != value)
            {
                MPState.CustomSongsEnabled = value;
                MPEvents.RaiseCustomSongsChanged(this, value);
            }

            customSongsToggle.Value = value;
            UpdateStates();
        }

        [UIAction("set-freemod")]
        public void SetFreeMod(bool value)
        {
            Plugin.Config.FreeMod = value;
            freeModToggle.Value = value;
            if (MPState.FreeModEnabled != value)
            {
                MPState.FreeModEnabled = value;
                MPEvents.RaiseFreeModChanged(this, value);
            }

            UpdateStates();
            SetModifierText();
        }

        [UIAction("set-host-pick")]
        public void SetHostPick(bool value)
        {
            hostPickToggle.Value = value;
            Plugin.Config.HostPick = value;
            if (MPState.HostPickEnabled != value)
            {
                MPState.HostPickEnabled = value;
                MPEvents.RaiseHostPickChanged(this, value);
            }

            UpdateStates();
        }

        [UIAction("set-vertical-hud")]
        public void SetVerticalHUD(bool value)
        {
            Plugin.Config.VerticalHUD = value;
            verticalHUDToggle.Value = value;

            Plugin.Config.SingleplayerHUD = !(!Plugin.Config.SingleplayerHUD || !value);
            defaultHUDToggle.Value = !(!Plugin.Config.SingleplayerHUD || !value);
        }

        [UIAction("set-default-hud")]
        public void SetDefaultHUD(bool value)
        {
            Plugin.Config.SingleplayerHUD = value;
            defaultHUDToggle.Value = value;

            Plugin.Config.VerticalHUD = Plugin.Config.VerticalHUD || value;
            verticalHUDToggle.Value = Plugin.Config.VerticalHUD || value;
        }

        [UIAction("set-hologram")]
        public void SetHologram(bool value)
        {
            Plugin.Config.Hologram = value;
            hologramToggle.Value = value;
        }

        [UIAction("set-lag-reducer")]
        public void SetLagReducer(bool value)
        {
            Plugin.Config.LagReducer = value;
            lagReducerToggle.Value = value;
        }

        [UIAction("set-miss-lighting")]
        public void SetMissLighting(bool value)
        {
            Plugin.Config.MissLighting = value;
            missLightingToggle.Value = value;
        }

        [UIAction("spawn-emote-panel")]
        private void SpawnEmotePanel()
        {
            emotePanel.ToggleActive();
        }
        #endregion
    }
}
