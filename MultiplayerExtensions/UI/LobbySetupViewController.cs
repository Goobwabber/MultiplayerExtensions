using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using Polyglot;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    public class LobbySetupViewController : BSMLResourceViewController, IInitializable, IDisposable
    {
        public override string ResourceName => "MultiplayerExtensions.UI.LobbySetupView.bsml";

        private IMultiplayerSessionManager sessionManager = null!;
        private HostLobbySetupViewController hostLobbySetupViewController = null!;
        private ClientLobbySetupViewController clientLobbySetupViewController = null!;

        private bool _isHost = false;
        CurvedTextMeshPro? modifierText;

        [Inject]
        private void Inject(IMultiplayerSessionManager sessionManager, HostLobbySetupViewController hostLobbySetupViewController, ClientLobbySetupViewController clientLobbySetupViewController)
        {
            this.sessionManager = sessionManager;
            this.hostLobbySetupViewController = hostLobbySetupViewController;
            this.clientLobbySetupViewController = clientLobbySetupViewController;
        }

        public void Initialize()
        {
            hostLobbySetupViewController.didActivateEvent += HostLobbySetupViewController_didActivateEvent;
            clientLobbySetupViewController.didActivateEvent += ClientLobbySetupViewController_didActivateEvent;
            sessionManager.playerStateChangedEvent += OnPlayerStateChanged;
        }

        public void Dispose()
        {
            hostLobbySetupViewController.didActivateEvent -= HostLobbySetupViewController_didActivateEvent;
            clientLobbySetupViewController.didActivateEvent -= ClientLobbySetupViewController_didActivateEvent;
            sessionManager.playerStateChangedEvent -= OnPlayerStateChanged;
        }

        private void HostLobbySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            IsHost = true;
        }

        private void ClientLobbySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            IsHost = false;

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
