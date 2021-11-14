using HMUI;
using IPA.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MultiplayerExtensions.Extensions
{
    class ExtendedPlayerTableCell : GameServerPlayerTableCell
    {
        protected ExtendedEntitlementChecker _entitlementChecker = null!;
        protected ILobbyPlayersDataModel _playersDataModel = null!;
        protected IMenuRpcManager _menuRpcManager = null!;

        private ButtonBinder __buttonBinder = new ButtonBinder();

        private static float alphaIsMe = 0.4f;
        private static float alphaIsNotMe = 0.2f;

        private static Color green = new Color(0f, 1f, 0f, 1f);
        private static Color yellow = new Color(0.125f, 0.75f, 1f, 1f);
        private static Color red = new Color(1f, 0f, 0f, 1f);
        private static Color normal = new Color(0.125f, 0.75f, 1f, 0.1f);

        private string lastLevelId = "";
        private IConnectedPlayer lastPlayer = null!;

        [Inject]
        internal void Inject(NetworkPlayerEntitlementChecker entitlementChecker, ILobbyPlayersDataModel playersDataModel, IMenuRpcManager menuRpcManager)
        {
            _entitlementChecker = (entitlementChecker as ExtendedEntitlementChecker)!;
            _playersDataModel = playersDataModel;
            _menuRpcManager = menuRpcManager;
        }

        internal void Construct(GameServerPlayerTableCell playerTableCell)
        {
            // Player
            _playerNameText = playerTableCell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_playerNameText");
            _localPlayerBackgroundImage = playerTableCell.GetField<Image, GameServerPlayerTableCell>("_localPlayerBackgroundImage");
            // Suggested Level
            _suggestedLevelText = playerTableCell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_suggestedLevelText");
            _suggestedCharacteristicIcon = playerTableCell.GetField<ImageView, GameServerPlayerTableCell>("_suggestedCharacteristicIcon");
            _suggestedDifficultyText = playerTableCell.GetField<TextMeshProUGUI, GameServerPlayerTableCell>("_suggestedDifficultyText");
            _emptySuggestedLevelText = playerTableCell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_emptySuggestedLevelText");
            // Suggested Modifiers
            _suggestedModifiersList = playerTableCell.GetField<GameplayModifierInfoListItemsList, GameServerPlayerTableCell>("_suggestedModifiersList");
            _emptySuggestedModifiersText = playerTableCell.GetField<CurvedTextMeshPro, GameServerPlayerTableCell>("_emptySuggestedModifiersText");
            // Buttons
            _kickPlayerButton = playerTableCell.GetField<Button, GameServerPlayerTableCell>("_kickPlayerButton");
            _useBeatmapButton = playerTableCell.GetField<Button, GameServerPlayerTableCell>("_useBeatmapButton");
            _useModifiersButton = playerTableCell.GetField<Button, GameServerPlayerTableCell>("_useModifiersButton");
            _useBeatmapButtonHoverHint = playerTableCell.GetField<HoverHint, GameServerPlayerTableCell>("_useBeatmapButtonHoverHint");
            // Status Icons
            _statusImageView = playerTableCell.GetField<ImageView, GameServerPlayerTableCell>("_statusImageView");
            _readyIcon = playerTableCell.GetField<Sprite, GameServerPlayerTableCell>("_readyIcon");
            _spectatingIcon = playerTableCell.GetField<Sprite, GameServerPlayerTableCell>("_spectatingIcon");
            _hostIcon = playerTableCell.GetField<Sprite, GameServerPlayerTableCell>("_hostIcon");
            // Helpers
            _gameplayModifiers = playerTableCell.GetField<GameplayModifiersModelSO, GameServerPlayerTableCell>("_gameplayModifiers");
            // TableCellWithSeparator
            _separator = playerTableCell.GetField<GameObject, TableCellWithSeparator>("_separator");
        }

        public override void Awake() {
            _menuRpcManager.setIsEntitledToLevelEvent += HandleSetIsEntitledToLevel;
            __buttonBinder.AddBinding(_kickPlayerButton, new Action(base.HandleKickPlayerButtonPressed));
            __buttonBinder.AddBinding(_useBeatmapButton, new Action(base.HandleUseBeatmapButtonPressed));
            __buttonBinder.AddBinding(_useModifiersButton, new Action(base.HandleUseModifiersButtonPressed));
        }

        public override void SetData(IConnectedPlayer connectedPlayer, ILobbyPlayerData playerData, bool hasKickPermissions, bool allowSelection, Task<AdditionalContentModel.EntitlementStatus> getLevelEntitlementTask)
        {
            if (getLevelEntitlementTask != null)
                getLevelEntitlementTask = getLevelEntitlementTask.ContinueWith<AdditionalContentModel.EntitlementStatus>(r => AdditionalContentModel.EntitlementStatus.Owned);
            base.SetData(connectedPlayer, playerData, hasKickPermissions, allowSelection, getLevelEntitlementTask);
            _localPlayerBackgroundImage.enabled = true;
            GetLevelEntitlement(connectedPlayer);
            lastPlayer = connectedPlayer;
        }

        private void GetLevelEntitlement(IConnectedPlayer player)
        {
            string? levelId = _playersDataModel.GetPlayerBeatmapLevel(_playersDataModel.partyOwnerId)?.levelID;
            if (levelId == null)
            {
                SetLevelEntitlement(player, EntitlementsStatus.Unknown);
                return;
            }

            lastLevelId = levelId;

            EntitlementsStatus entitlement = EntitlementsStatus.Unknown;
            if (!player.isMe)
                entitlement = _entitlementChecker.GetUserEntitlementStatusWithoutRequest(player.userId, levelId);
            if (entitlement != EntitlementsStatus.Unknown)
                SetLevelEntitlement(player, entitlement);
            else
            {
                _entitlementChecker.GetUserEntitlementStatus(player.userId, levelId);
            }
        }

        private void SetLevelEntitlement(IConnectedPlayer player, EntitlementsStatus status)
        {
            //Plugin.Log?.Debug($"{player.userId} has entitlement {status.ToString()}");
            Color backgroundColor = status switch
            {
                EntitlementsStatus.Ok => green,
                EntitlementsStatus.NotOwned => red,
                _ => normal,
            };

            backgroundColor.a = player.isMe ? alphaIsMe : alphaIsNotMe;
            _localPlayerBackgroundImage.color = backgroundColor;
        }

        private void HandleSetIsEntitledToLevel(string userId, string levelId, EntitlementsStatus status)
        {
            if (userId == lastPlayer?.userId && levelId == lastLevelId)
            {
                SetLevelEntitlement(lastPlayer, status);
            }
        }
    }
}
