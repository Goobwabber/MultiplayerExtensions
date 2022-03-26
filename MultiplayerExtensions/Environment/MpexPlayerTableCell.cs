using MultiplayerCore.Objects;
using SiraUtil.Affinity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MultiplayerExtensions.Objects
{
    public class MpexPlayerTableCell : IInitializable, IDisposable, IAffinity
    {
        private readonly ServerPlayerListViewController _playerListView;
        private readonly MpEntitlementChecker _entitlementChecker;
        private readonly ILobbyPlayersDataModel _playersDataModel;
        private readonly IMenuRpcManager _menuRpcManager;

        private static float alphaIsMe = 0.4f;
        private static float alphaIsNotMe = 0.2f;

        private static Color green = new Color(0f, 1f, 0f, 1f);
        private static Color yellow = new Color(0.125f, 0.75f, 1f, 1f);
        private static Color red = new Color(1f, 0f, 0f, 1f);
        private static Color normal = new Color(0.125f, 0.75f, 1f, 0.1f);

        internal MpexPlayerTableCell(
            ServerPlayerListViewController playerListView,
            NetworkPlayerEntitlementChecker entitlementChecker,
            ILobbyPlayersDataModel playersDataModel,
            IMenuRpcManager menuRpcManager)
        {
            _playerListView = playerListView;
            _entitlementChecker = (entitlementChecker as MpEntitlementChecker)!;
            _playersDataModel = playersDataModel;
            _menuRpcManager = menuRpcManager;
        }

        public void Initialize() 
        {
            _menuRpcManager.setIsEntitledToLevelEvent += HandleSetIsEntitledToLevel;
        }

        public void Dispose()
        {
            _menuRpcManager.setIsEntitledToLevelEvent -= HandleSetIsEntitledToLevel;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameServerPlayerTableCell), nameof(GameServerPlayerTableCell.SetData))]
        public void SetDataPrefix(IConnectedPlayer connectedPlayer, ILobbyPlayerData playerData, bool hasKickPermissions, bool allowSelection, Task<AdditionalContentModel.EntitlementStatus> getLevelEntitlementTask, Image ____localPlayerBackgroundImage)
        {
            if (getLevelEntitlementTask != null)
                getLevelEntitlementTask = Task.FromResult(AdditionalContentModel.EntitlementStatus.Owned);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameServerPlayerTableCell), nameof(GameServerPlayerTableCell.SetData))]
        public void SetDataPostfix(IConnectedPlayer connectedPlayer, ILobbyPlayerData playerData, bool hasKickPermissions, bool allowSelection, Task<AdditionalContentModel.EntitlementStatus> getLevelEntitlementTask, Image ____localPlayerBackgroundImage)
        {
            ____localPlayerBackgroundImage.enabled = true;
            string? hostSelectedLevel = _playersDataModel[_playersDataModel.partyOwnerId].beatmapLevel?.beatmapLevel?.levelID;
            if (hostSelectedLevel == null)
            {
                SetLevelEntitlement(____localPlayerBackgroundImage, EntitlementsStatus.Unknown);
                return;
            }

            EntitlementsStatus entitlement = EntitlementsStatus.Unknown;
            if (!connectedPlayer.isMe)
                entitlement = _entitlementChecker.GetUserEntitlementStatusWithoutRequest(connectedPlayer.userId, hostSelectedLevel);
            // TODO: change color for local player
            if (entitlement != EntitlementsStatus.Unknown)
                SetLevelEntitlement(____localPlayerBackgroundImage, entitlement);
            else if (!connectedPlayer.isMe)
            {
                // This might be a bad idea, race condition can cause packets that scale with the amount of players
                _entitlementChecker.GetUserEntitlementStatus(connectedPlayer.userId, hostSelectedLevel);
            }
        }

        private void SetLevelEntitlement(Image backgroundImage, EntitlementsStatus status)
        {
            Color backgroundColor = status switch
            {
                EntitlementsStatus.Ok => green,
                EntitlementsStatus.NotOwned => red,
                _ => normal,
            };

            backgroundColor.a = alphaIsNotMe;
            backgroundImage.color = backgroundColor;
        }

        private void HandleSetIsEntitledToLevel(string userId, string levelId, EntitlementsStatus status)
            => _playerListView.SetDataToTable();
    }
}
