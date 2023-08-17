using System;
using System.Collections.Generic;
using HMUI;
using MultiplayerCore.Players;
using MultiplayerExtensions.Players;
using MultiplayerExtensions.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MultiplayerExtensions.Environments.Lobby
{
    public class MpexAvatarNameTag : MonoBehaviour
    {
        enum PlayerIconSlot
        {
            Platform = 0
        }
        
        private readonly Dictionary<PlayerIconSlot, ImageView> _playerIcons = new();

        private IConnectedPlayer _player = null!;
        private MpPlayerManager _playerManager = null!;
        private MpexPlayerManager _mpexPlayerManager = null!;
        private SpriteManager _spriteManager = null!;
        private ImageView _bg = null!;
        private CurvedTextMeshPro _nameText = null!;

        [Inject]
        internal void Construct(
            IConnectedPlayer player,
            MpPlayerManager playerManager,
            MpexPlayerManager mpexPlayerManager,
            SpriteManager spriteManager)
        {
            _player = player;
            _playerManager = playerManager;
            _mpexPlayerManager = mpexPlayerManager;
            _spriteManager = spriteManager;
        }

        private void Awake()
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

            // Set player data
            _nameText.text = _player.userName;
            _nameText.color = Color.white;
            if (_mpexPlayerManager.TryGetPlayer(_player.userId, out var mpexData))
                _nameText.color = mpexData.Color;
            if (_playerManager.TryGetPlayer(_player.userId, out var data))
                SetPlatformData(data);
        }

        private void OnEnable()
        {
            _playerManager.PlayerConnectedEvent += HandlePlatformData;
            _mpexPlayerManager.PlayerConnectedEvent += HandleMpexData;
        }

        private void OnDisable()
        {
            _playerManager.PlayerConnectedEvent -= HandlePlatformData;
            _mpexPlayerManager.PlayerConnectedEvent -= HandleMpexData;
        }

        private void HandlePlatformData(IConnectedPlayer player, MpPlayerData data)
        {
            if (player == _player)
                SetPlatformData(data);
        }

        private void HandleMpexData(IConnectedPlayer player, MpexPlayerData data)
        {
            if (player == _player)
                _nameText.color = data.Color;
        }

        private void SetPlatformData(MpPlayerData data)
        {
            Sprite icon = null;
            switch (data.Platform)
            {
                case Platform.Steam:
                    icon = _spriteManager.IconSteam64;
                    break;
                case Platform.OculusQuest:
                    icon = _spriteManager.IconMeta64;
                    break;
                case Platform.OculusPC:
                    icon = _spriteManager.IconOculus64;
                    break;
                default:
                    icon = _spriteManager.IconToaster64;
                    break;
            }
            SetIcon(PlayerIconSlot.Platform, icon);
        }

        private void SetIcon(PlayerIconSlot slot, Sprite sprite)
        {
            if (!_playerIcons.TryGetValue(slot, out ImageView imageView))
            {
                var iconObj = new GameObject($"MpexPlayerIcon({slot})");
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
    }
}