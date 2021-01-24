using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MultiplayerExtensions.OverrideClasses
{
    class PlayerTableCellStub : GameServerPlayerTableCell
    {
        [Inject]
        protected readonly NetworkPlayerEntitlementChecker _entitlementChecker;

        [Inject]
        protected readonly ILobbyPlayersDataModel _playersDataModel;

        [Inject]
        protected readonly IMenuRpcManager _menuRpcManager;

        private ButtonBinder __buttonBinder = new ButtonBinder();
        private CancellationTokenSource entitlementCts;

        private static Color green = new Color(0f, 1f, 0f, 1f);
        private static Color yellow = new Color(0.125f, 0.75f, 1f, 1f);
        private static Color red = new Color(1f, 0f, 0f, 1f);
        private static Color normal = new Color(0.125f, 0.75f, 1f, 0.1f);

        private string lastLevelId = "";
        private IConnectedPlayer lastPlayer;

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

        public override void SetData(IConnectedPlayer connectedPlayer, ILobbyPlayerDataModel playerDataModel, bool isHost, Task<AdditionalContentModel.EntitlementStatus> getLevelEntitlementTask)
        {
            base.SetData(connectedPlayer, playerDataModel, isHost, getLevelEntitlementTask);
            GetLevelEntitlement(connectedPlayer);
            lastPlayer = connectedPlayer;
        }

        private async void GetLevelEntitlement(IConnectedPlayer player)
        {
            if (entitlementCts != null)
                entitlementCts.Cancel();
            entitlementCts = new CancellationTokenSource();

            string? levelId = _playersDataModel.GetPlayerBeatmapLevel(_playersDataModel.hostUserId)?.levelID;
            if (levelId == null)
                return;

            lastLevelId = levelId;
            EntitlementsStatus entitlement = player.isMe ? await _entitlementChecker.GetEntitlementStatus(levelId) : await _entitlementChecker.GetTcsTaskCanPlayerPlayLevel(player, levelId, entitlementCts.Token, out _);
            SetLevelEntitlement(player, entitlement);
        }

        private void SetLevelEntitlement(IConnectedPlayer player, EntitlementsStatus status)
        {
            Color backgroundColor = status switch
            {
                EntitlementsStatus.Ok => green,
                EntitlementsStatus.NotOwned => red,
                _ => normal,
            };

            backgroundColor.a = player.isMe ? 0.4f : 0.1f;
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
