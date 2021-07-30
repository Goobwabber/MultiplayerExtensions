using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using Polyglot;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    [HotReload(@"C:\Users\rithik\source\repos\MultiplayerExtensions\MultiplayerExtensions\UI\LobbySetupView.bsml")]
    [ViewDefinition("MultiplayerExtensions.UI.LobbySetupView.bsml")]
    public class MPLobbySetupViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private IMultiplayerSessionManager sessionManager = null!;
        private LobbySetupViewController lobbySetupViewController = null!;
        CurvedTextMeshPro? modifierText;
        private bool _isHost;

        [Inject]
        private void Inject(IMultiplayerSessionManager sessionManager, LobbySetupViewController lobbySetupViewController)
        {
            this.sessionManager = sessionManager;
            this.lobbySetupViewController = lobbySetupViewController;
        }

        public void Initialize()
        {
            lobbySetupViewController.didActivateEvent += LobbySetupViewController_didActivateEvent;
            sessionManager.playerStateChangedEvent += OnPlayerStateChanged;
        }

        public void Dispose()
        {
            lobbySetupViewController.didActivateEvent -= LobbySetupViewController_didActivateEvent;
            sessionManager.playerStateChangedEvent -= OnPlayerStateChanged;
        }

        private void LobbySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            IsHost = lobbySetupViewController.GetField<bool, LobbySetupViewController>("_isPartyOwner");

            if (sessionManager.connectionOwner != null)
                OnPlayerStateChanged(sessionManager.connectionOwner);
        }

        private void UpdateStates()
        {
            if (IsHost)
            {
                sessionManager?.SetLocalPlayerState("customsongs", CustomSongs);
                sessionManager?.SetLocalPlayerState("freemod", FreeMod);
                sessionManager?.SetLocalPlayerState("hostpick", HostPick);
            }
        }

        private void OnPlayerStateChanged(IConnectedPlayer player)
        {
            if (!IsHost)
            {
                if (player.userId != sessionManager.localPlayer.userId && player.isConnectionOwner)
                {
                    CustomSongs = player.HasState("customsongs");
                    FreeMod = player.HasState("freemod");
                    HostPick = player.HasState("hostpick");
                }
            }
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

        [UIValue("custom-songs")]
        private bool CustomSongs
        {
            get => MPState.CustomSongsEnabled;
            set
            {
                MPState.CustomSongsEnabled = value;
                MPEvents.RaiseCustomSongsChanged(this, value);
                UpdateStates();
                NotifyPropertyChanged(nameof(CustomSongs));
            }
        }

        [UIValue("freemod")]
        private bool FreeMod
        {
            get => MPState.FreeModEnabled;
            set
            {
                MPState.FreeModEnabled = value;
                MPEvents.RaiseFreeModChanged(this, value);
                UpdateStates();
                NotifyPropertyChanged(nameof(FreeMod));
                SetModifierText();
            }
        }

        [UIValue("host-pick")]
        private bool HostPick
        {
            get => MPState.HostPickEnabled;
            set
            {
                MPState.HostPickEnabled = value;
                MPEvents.RaiseHostPickChanged(this, value);
                UpdateStates();
                NotifyPropertyChanged(nameof(HostPick));
            }
        }

        [UIValue("is-host")]
        private bool IsHost
        {
            get => _isHost;
            set
            {
                _isHost = value;
                NotifyPropertyChanged(nameof(IsHost));
            }
        }
    }
}
