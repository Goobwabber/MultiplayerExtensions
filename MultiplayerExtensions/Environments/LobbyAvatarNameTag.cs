using System.Collections.Generic;
using HMUI;
using MultiplayerExtensions.Extensions;
using MultiplayerExtensions.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerExtensions.Environments
{
    public class LobbyAvatarNameTag : MonoBehaviour
    {
        enum PlayerIconSlot
        {
            Platform = 0
        }
        
        private bool _enabled;
        private IConnectedPlayer? _playerInfo;
        private Dictionary<PlayerIconSlot, ImageView> _playerIcons;

        private ImageView _bg = null!;
        private CurvedTextMeshPro _nameText = null!;

        public LobbyAvatarNameTag()
        {
            _enabled = false;
            _playerInfo = null;
            _playerIcons = new Dictionary<PlayerIconSlot, ImageView>();
        }

        public void Awake()
        {
            // Get references
            _bg = transform.Find("BG").GetComponent<ImageView>();
            _nameText = transform.Find("Name").GetComponent<CurvedTextMeshPro>();
            
            // Enable horizontal layout on bg
            if (!_bg.TryGetComponent<HorizontalLayoutGroup>(out _))
            {
                var hLayout = _bg.gameObject.AddComponent<HorizontalLayoutGroup>();
                hLayout.childAlignment = TextAnchor.MiddleCenter;
                hLayout.childForceExpandWidth = false;
                hLayout.childForceExpandHeight = false;
                hLayout.childScaleWidth = false;
                hLayout.childScaleHeight = false;
                hLayout.spacing = 4f;
            }

            // Re-nest name onto bg
            _nameText.transform.SetParent(_bg.transform, false);
            
            // Take control of name tag
            if (_nameText.TryGetComponent<ConnectedPlayerName>(out var nativeNameScript))
                Destroy(nativeNameScript);
            _nameText.text = "Player";
        }

        public void OnEnable()
        {
            _enabled = true;
            
            if (_playerInfo != null) 
                SetPlayerInfo(_playerInfo);
        }

        #region Set Player Info
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

            switch (extendedPlayer.platform)
            {
                case Platform.Steam:
                    SetIcon(PlayerIconSlot.Platform, Sprites.IconSteam64);
                    break;
                case Platform.OculusQuest:
                case Platform.OculusPC:
                    SetIcon(PlayerIconSlot.Platform, Sprites.IconOculus64);
                    break;
                default:
                    RemoveIcon(PlayerIconSlot.Platform);
                    break;
            }
        }

        private void SetSimplePlayerInfo(IConnectedPlayer simplePlayer)
        {
            _playerInfo = simplePlayer;

            if (!_enabled)
                return;
            
            _nameText.text = simplePlayer.userName;
            _nameText.color = Color.white;
            
            RemoveIcon(PlayerIconSlot.Platform);
        }
        #endregion

        #region Set Icons

        private void SetIcon(PlayerIconSlot slot, Sprite sprite)
        {
            if (!_enabled)
                return;
            
            if (!_playerIcons.TryGetValue(slot, out ImageView imageView))
            {
                var iconObj = new GameObject($"MpExPlayerIcon({slot})");
                iconObj.transform.SetParent(_bg.transform, false);
                iconObj.transform.SetSiblingIndex((int)slot);
                iconObj.layer = 5;

                iconObj.AddComponent<CanvasRenderer>();
                
                imageView = iconObj.AddComponent<ImageView>();
                imageView.maskable = true;
                imageView.fillCenter = true;
                imageView.preserveAspect = true;
                imageView.material = _bg.material; // No Glow Billboard material
                _playerIcons[slot] = imageView;

                var rectTransform = iconObj.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(3.2f, 3.2f);
            }

            imageView.sprite = sprite;
            
            _nameText.transform.SetSiblingIndex(999);
        }

        private void RemoveIcon(PlayerIconSlot slot)
        {
            if (!_enabled)
                return;
            
            if (_playerIcons.TryGetValue(slot, out var imageView))
            {
                Destroy(imageView.gameObject);
                _playerIcons.Remove(slot);
            }
        }
        #endregion
    }
}