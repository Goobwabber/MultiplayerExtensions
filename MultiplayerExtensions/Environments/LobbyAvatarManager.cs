using System;
using System.Collections.Generic;
using IPA.Utilities;
using MultiplayerExtensions.Extensions;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class LobbyAvatarManager : IInitializable, IDisposable
    {
        protected readonly ExtendedSessionManager _sessionManager;
        protected readonly MultiplayerLobbyAvatarManager _avatarManager;
        
        protected Dictionary<string, MultiplayerLobbyAvatarController>? _refPlayerIdToAvatarMap;

        private Dictionary<string, ExtendedPlayer> _extendedPlayers;
        
        internal LobbyAvatarManager(IMultiplayerSessionManager sessionManager, MultiplayerLobbyAvatarManager avatarManager)
        {
            _sessionManager = (sessionManager as ExtendedSessionManager)!;
            _avatarManager = avatarManager;
            
            _refPlayerIdToAvatarMap = null;

            _extendedPlayers = new Dictionary<string, ExtendedPlayer>();
        }
        
        public void Initialize()
        {
            MPEvents.LobbyAvatarCreated += HandleLobbyAvatarCreated;
            _sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
            _sessionManager.extendedPlayerConnectedEvent += HandleExtendedPlayerConnected;
        }

        public void Dispose()
        {
            MPEvents.LobbyAvatarCreated -= HandleLobbyAvatarCreated;
            _sessionManager.playerDisconnectedEvent -= HandlePlayerDisconnected;
            _sessionManager.extendedPlayerConnectedEvent -= HandleExtendedPlayerConnected;
        }

        #region Events
        private void HandleLobbyAvatarCreated(object sender, IConnectedPlayer player)
        {
            if (_extendedPlayers.ContainsKey(player.userId))
                player = _extendedPlayers[player.userId];
            CreateOrUpdateNameTag(player);
        }

        private void HandleExtendedPlayerConnected(ExtendedPlayer player)
        {
            // This packet is usually received before the avatar is actually created
            _extendedPlayers[player.userId] = player;
            CreateOrUpdateNameTag(player);
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            _extendedPlayers.Remove(player.userId);
        }
        #endregion
        
        #region NameTag
        private void CreateOrUpdateNameTag(IConnectedPlayer player)
        {
            var objAvatarCaption = GetAvatarCaptionObject(player.userId);
            if (objAvatarCaption == null)
                return;

            LobbyAvatarNameTag nameTag;
            if (!objAvatarCaption.TryGetComponent(out nameTag))
                nameTag = objAvatarCaption.AddComponent<LobbyAvatarNameTag>();
            
            nameTag.SetPlayerInfo(player);
        }
        #endregion

        #region Helpers
        private MultiplayerLobbyAvatarController? GetAvatarController(string userId)
        {
            if (_refPlayerIdToAvatarMap == null)
                _refPlayerIdToAvatarMap = _avatarManager.GetField<Dictionary<string, MultiplayerLobbyAvatarController>,
                    MultiplayerLobbyAvatarManager>("_playerIdToAvatarMap");
            
            if (_refPlayerIdToAvatarMap != null)
                return _refPlayerIdToAvatarMap.TryGetValue(userId, out var value) ? value : null;

            return null;
        }

        private GameObject? GetAvatarCaptionObject(string userId)
        {
            return GetAvatarController(userId)?.transform.Find("AvatarCaption").gameObject;
        }
        #endregion
    }
}