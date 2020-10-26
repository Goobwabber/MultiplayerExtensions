using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using MultiplayerExtensions.HarmonyPatches;
using HMUI;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;

namespace MultiplayerExtensions.UI
{
    internal class GameplaySetupPanel : NotifiableSingleton<GameplaySetupPanel>
    {
        MultiplayerSessionManager sessionManager;

        public void UpdatePanel()
        {
            var isMultiplayer = LobbyJoinPatch.IsMultiplayer;
            var isHost = LobbyJoinPatch.IsMultiplayer;
            sessionManager = Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();
            verticalHUDToggle.interactable = isMultiplayer;
            defaultHUDToggle.interactable = isMultiplayer;
            hologramToggle.interactable = isMultiplayer;
            customSongsToggle.interactable = isHost && isMultiplayer;
            enforceModsToggle.interactable = isHost && isMultiplayer && !CustomSongs;
        }

        private void UpdateStates()
        {
            sessionManager.SetLocalPlayerState("customsongs", CustomSongs);
            sessionManager.SetLocalPlayerState("enforcemods", EnforceMods);
        }

        [UIComponent("VerticalHUDToggle")]
        public ToggleSetting verticalHUDToggle;
        [UIValue("VerticalHUD")]
        public bool VerticalHUD
        {
            get => Plugin.Config.VerticalHUD;
            set
            {
                Plugin.Config.VerticalHUD = value;
            }
        }
        [UIAction("SetVerticalHUD")]
        public void SetVerticalHUD(bool value)
        {
            VerticalHUD = value;
            SetDefaultHUD(!(!DefaultHUD || !value));
            defaultHUDToggle.interactable = value;
        }

        [UIComponent("DefaultHUDToggle")]
        public ToggleSetting defaultHUDToggle;
        [UIValue("DefaultHUD")]
        public bool DefaultHUD
        {
            get => Plugin.Config.SingleplayerHUD;
            set
            {
                Plugin.Config.SingleplayerHUD = value;
            }
        }
        [UIAction("SetDefaultHUD")]
        public void SetDefaultHUD(bool value)
        {
            DefaultHUD = value;
        }

        [UIComponent("HologramToggle")]
        public ToggleSetting hologramToggle;
        [UIValue("Hologram")]
        public bool Hologram
        {
            get => Plugin.Config.Hologram;
            set
            {
                Plugin.Config.Hologram = value;
            }
        }
        [UIAction("SetHologram")]
        public void SetHologram(bool value)
        {
            Hologram = value;
        }

        [UIComponent("CustomSongsToggle")]
        public ToggleSetting customSongsToggle;
        [UIValue("CustomSongs")]
        public bool CustomSongs
        {
            get => Plugin.Config.CustomSongs;
            set
            {
                Plugin.Config.CustomSongs = value;
            }
        }
        [UIAction("SetCustomSongs")]
        public void SetCustomSongs(bool value)
        {
            CustomSongs = value;
            SetEnforceMods(EnforceMods || value);
            enforceModsToggle.interactable = !value;
            UpdateStates();
        }

        [UIComponent("EnforceModsToggle")]
        public ToggleSetting enforceModsToggle;
        [UIValue("EnforceMods")]
        public bool EnforceMods
        {
            get => Plugin.Config.EnforceMods;
            set
            {
                Plugin.Config.EnforceMods = value;
            }
        }
        [UIAction("SetEnforceMods")]
        public void SetEnforceMods(bool value)
        {
            EnforceMods = value;
            UpdateStates();
        }
    }
}
