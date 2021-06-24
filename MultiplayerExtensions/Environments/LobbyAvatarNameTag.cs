using System;
using HMUI;
using MultiplayerExtensions.Sessions;
using UnityEngine;

namespace MultiplayerExtensions.Environments
{
    public class LobbyAvatarNameTag : MonoBehaviour
    {
        private bool _enabled;
        private CurvedTextMeshPro _nameText;
        private IConnectedPlayer? _playerInfo;

        public void Awake()
        {
            var nameObject = transform.Find("Name");
            
            if (nameObject.TryGetComponent<ConnectedPlayerName>(out var nativeNameScript))
                Destroy(nativeNameScript);
            
            _nameText = nameObject.GetComponent<CurvedTextMeshPro>();
            _nameText.text = "Player";
        }

        public void OnEnable()
        {
            _enabled = true;
            
            if (_playerInfo != null) 
                SetPlayerInfo(_playerInfo);
        }

        public void SetPlayerInfo(IConnectedPlayer player)
        {
            if (player is ExtendedPlayer extendedPlayer)
                SetExtendedPlayerInfo(extendedPlayer);
            else
                SetSimplePlayerInfo(player);
        }

        private void SetExtendedPlayerInfo(ExtendedPlayer extendedPlayer)
        {
            _playerInfo = extendedPlayer;

            if (!_enabled)
                return;

            _nameText.text = extendedPlayer.userName;
            _nameText.color = extendedPlayer.playerColor;
        }

        private void SetSimplePlayerInfo(IConnectedPlayer simplePlayer)
        {
            _playerInfo = simplePlayer;

            if (!_enabled)
                return;
            
            _nameText.text = simplePlayer.userName;
            _nameText.color = Color.white;
        }
    }
}