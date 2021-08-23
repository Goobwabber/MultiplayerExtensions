using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MultiplayerExtensions.Extensions;
using Polyglot;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    class LobbySetupPanel : BSMLResourceViewController
    {
        public override string ResourceName => "MultiplayerExtensions.UI.LobbySetupPanel.bsml";

        private ExtendedSessionManager _sessionManager = null!;
        private LobbyPlayerPermissionsModel _permissionsModel = null!;

        CurvedTextMeshPro? modifierText;

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, LobbyPlayerPermissionsModel permissionsModel, LobbySetupViewController lobbyViewController)
        {
            this._sessionManager = (sessionManager as ExtendedSessionManager)!;
            this._permissionsModel = permissionsModel;
            base.DidActivate(true, false, true);

            lobbyViewController.didActivateEvent += OnActivate;
            _sessionManager.playerStateChangedEvent += HandlePlayerStateChanged;
        }

        #region UIComponents
        [UIComponent("CustomSongsToggle")]
        public ToggleSetting customSongsToggle = null!;

        [UIComponent("FreeModToggle")]
        public ToggleSetting freeModToggle = null!;

        [UIComponent("HostPickToggle")]
        public ToggleSetting hostPickToggle = null!;

        [UIComponent("DefaultHUDToggle")]
        public ToggleSetting defaultHUDToggle = null!;

        [UIComponent("HologramToggle")]
        public ToggleSetting hologramToggle = null!;

        [UIComponent("LagReducerToggle")]
        public ToggleSetting lagReducerToggle = null!;

        [UIComponent("MissLightingToggle")]
        public ToggleSetting missLightingToggle = null!;

        [UIComponent("DownloadProgressText")]
        public FormattableText downloadProgressText = null!;
        #endregion

        #region UIValues
        [UIValue("FreeMod")]
        public bool FreeMod
        {
            get => _permissionsModel.isPartyOwner ? Plugin.Config.FreeMod : MPState.FreeModEnabled;
            set {
                if (_permissionsModel.isPartyOwner)
                    Plugin.Config.FreeMod = value;
                if (MPState.FreeModEnabled != value)
                {
                    MPState.FreeModEnabled = value;
                    MPEvents.RaiseFreeModChanged(this, value);
                }
            }
        }

        [UIValue("HostPick")]
        public bool HostPick
        {
            get => Plugin.Config.HostPick;
            set
            {
                if (_permissionsModel.isPartyOwner)
                    Plugin.Config.HostPick = value;
                if (MPState.HostPickEnabled != value)
                {
                    MPState.HostPickEnabled = value;
                    MPEvents.RaiseHostPickChanged(this, value);
                }
            }
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

        [UIValue("LagReducer")]
        public bool LagReducer
        {
            get => Plugin.Config.LagReducer;
            set { Plugin.Config.LagReducer = value; }
        }

        [UIValue("MissLighting")]
        public bool MissLighting
        {
            get => Plugin.Config.MissLighting;
            set { Plugin.Config.MissLighting = value; }
        }

        [UIValue("DownloadProgress")]
        public string DownloadProgress
        {
            get => downloadProgressText.text;
            set { downloadProgressText.text = value; }
        }
        #endregion

        #region UIActions
        [UIAction("SetFreeMod")]
        public void SetFreeMod(bool value)
        {
            FreeMod = value;
            freeModToggle.Value = value;

            UpdateStates();
            SetModifierText();
        }

        [UIAction("SetHostPick")]
        public void SetHostPick(bool value)
        {
            HostPick = value;
            hostPickToggle.Value = value;

            UpdateStates();
        }

        [UIAction("SetDefaultHUD")]
        public void SetDefaultHUD(bool value)
        {
            DefaultHUD = value;
            defaultHUDToggle.Value = value;

            //VerticalHUD = VerticalHUD || value;
            //verticalHUDToggle.Value = VerticalHUD || value;
        }

        [UIAction("SetHologram")]
        public void SetHologram(bool value)
        {
            Hologram = value;
            hologramToggle.Value = value;
        }

        [UIAction("SetLagReducer")]
        public void SetLagReducer(bool value)
        {
            LagReducer = value;
            lagReducerToggle.Value = value;
        }

        [UIAction("SetMissLighting")]
        public void SetMissLighting(bool value)
        {
            MissLighting = value;
            missLightingToggle.Value = value;
        }
        #endregion

        private void OnActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            freeModToggle.interactable = _permissionsModel.isPartyOwner;
            hostPickToggle.interactable = _permissionsModel.isPartyOwner;

            if (firstActivation)
            {
                Transform spectatorText = transform.Find("Wrapper").Find("SpectatorModeWarningText");
                spectatorText.position = new Vector3(spectatorText.position.x, 0.25f, spectatorText.position.z);
            }
        }

        private void HandlePlayerStateChanged(IConnectedPlayer player)
		{
            ExtendedPlayer? exPlayer = _sessionManager.GetExtendedPlayer(player);
            if (exPlayer != null && exPlayer.isPartyOwner)
			{
                FreeMod = exPlayer.HasState("freemod");
                HostPick = exPlayer.HasState("hostpick");
            }
		}

        private void UpdateStates()
        {
            _sessionManager?.SetLocalPlayerState("freemod", FreeMod);
            _sessionManager?.SetLocalPlayerState("hostpick", HostPick);
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
    }
}