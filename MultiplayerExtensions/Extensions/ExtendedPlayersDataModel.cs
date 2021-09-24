using BeatSaverSharp.Models;
using MultiplayerExtensions.Beatmaps;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Extensions
{
    class ExtendedPlayersDataModel : LobbyPlayersDataModel, ILobbyPlayersDataModel, IDisposable
    {
        protected readonly PacketManager _packetManager;
        protected readonly ExtendedSessionManager _sessionManager;

        internal ExtendedPlayersDataModel(PacketManager packetManager, IMultiplayerSessionManager sessionManager)
        {
            _packetManager = packetManager;
            _sessionManager = (sessionManager as ExtendedSessionManager)!;
        }

        public override void Activate()
        {
            this._packetManager.RegisterCallback<PreviewBeatmapPacket>(this.HandlePreviewBeatmapPacket);
            base.Activate();

            _menuRpcManager.recommendBeatmapEvent -= base.HandleMenuRpcManagerRecommendBeatmap;
            _menuRpcManager.recommendBeatmapEvent += this.HandleMenuRpcManagerRecommendBeatmap;
            _menuRpcManager.getRecommendedBeatmapEvent -= base.HandleMenuRpcManagerGetRecommendedBeatmap;
            _menuRpcManager.getRecommendedBeatmapEvent += this.HandleMenuRpcManagerGetRecommendedBeatmap;
            _menuRpcManager.clearRecommendedBeatmapEvent -= base.HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.clearRecommendedBeatmapEvent += this.HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.recommendGameplayModifiersEvent -= base.HandleMenuRpcManagerRecommendGameplayModifiers;
            _menuRpcManager.recommendGameplayModifiersEvent += this.HandleMenuRpcManagerRecommendGameplayModifiers;
            _menuRpcManager.clearRecommendedGameplayModifiersEvent -= base.HandleMenuRpcManagerClearRecommendedGameplayModifiers;
            _menuRpcManager.clearRecommendedGameplayModifiersEvent += this.HandleMenuRpcManagerClearRecommendedGameplayModifiers;
            _menuRpcManager.setPlayersPermissionConfigurationEvent -= base.HandleMenuRpcManagerSetPlayersPermissionConfiguration;
            _menuRpcManager.setPlayersPermissionConfigurationEvent += this.HandleMenuRpcManagerSetPlayersPermissionConfiguration;
        }

        public override void Deactivate()
        {
            this._packetManager.UnregisterCallback<PreviewBeatmapPacket>();

            _menuRpcManager.recommendBeatmapEvent -= this.HandleMenuRpcManagerRecommendBeatmap;
            _menuRpcManager.recommendBeatmapEvent += base.HandleMenuRpcManagerRecommendBeatmap;
            _menuRpcManager.getRecommendedBeatmapEvent -= this.HandleMenuRpcManagerGetRecommendedBeatmap;
            _menuRpcManager.getRecommendedBeatmapEvent += base.HandleMenuRpcManagerGetRecommendedBeatmap;
            _menuRpcManager.clearRecommendedBeatmapEvent -= this.HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.clearRecommendedBeatmapEvent += base.HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.recommendGameplayModifiersEvent -= this.HandleMenuRpcManagerRecommendGameplayModifiers;
            _menuRpcManager.recommendGameplayModifiersEvent += base.HandleMenuRpcManagerRecommendGameplayModifiers;
            _menuRpcManager.clearRecommendedGameplayModifiersEvent -= this.HandleMenuRpcManagerClearRecommendedGameplayModifiers;
            _menuRpcManager.clearRecommendedGameplayModifiersEvent += base.HandleMenuRpcManagerClearRecommendedGameplayModifiers;
            _menuRpcManager.setPlayersPermissionConfigurationEvent -= this.HandleMenuRpcManagerSetPlayersPermissionConfiguration;
            _menuRpcManager.setPlayersPermissionConfigurationEvent += base.HandleMenuRpcManagerSetPlayersPermissionConfiguration;

            base.Deactivate();
        }

        public new void Dispose()
        {
            this.Deactivate();
        }

        /// <summary>
        /// Handles a <see cref="MultiplayerExtensions.Beatmaps.PreviewBeatmapPacket"/> used to transmit data about a custom song.
        /// </summary>
        private void HandlePreviewBeatmapPacket(PreviewBeatmapPacket packet, IConnectedPlayer player)
        {
            string? hash = Utilities.Utils.LevelIdToHash(packet.levelId);
            if (hash != null)
            {
                Plugin.Log?.Debug($"'{player.userId}' selected song '{hash}'.");
                BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(packet.characteristic);
                PreviewBeatmapStub preview = new PreviewBeatmapStub(packet);
                base.SetPlayerBeatmapLevel(player.userId, preview, packet.difficulty, characteristic);
            }
        }

        /// <summary>
        /// Used to raise the <see cref="MultiplayerExtensions.MPEvents.BeatmapSelected"/> event.
        /// </summary>
        public override void HandleMenuRpcManagerClearBeatmap(string userId)
        {
            OnSelectedBeatmap(userId, null);
            base.HandleMenuRpcManagerClearBeatmap(userId);
        }

        /// <summary>
        /// Triggered when a player joins and sends the request.
        /// If the newly joined player is not modded or the selected song isn't custom, sends back a vanilla packet.
        /// Otherwise, sends a <see cref="MultiplayerExtensions.Beatmaps.PreviewBeatmapPacket"/>
        /// </summary>
        public override void HandleMenuRpcManagerGetRecommendedBeatmap(string userId)
        {
            ILobbyPlayerData lobbyPlayerDataModel = this.GetLobbyPlayerDataModel(this.localUserId);
            if (lobbyPlayerDataModel != null && lobbyPlayerDataModel.beatmapLevel != null)
			{       
                if (lobbyPlayerDataModel.beatmapLevel is PreviewBeatmapStub preview)
                    _packetManager.Send(new PreviewBeatmapPacket(preview, lobbyPlayerDataModel.beatmapCharacteristic.serializedName, lobbyPlayerDataModel.beatmapDifficulty));
                this._menuRpcManager.RecommendBeatmap(new BeatmapIdentifierNetSerializable(lobbyPlayerDataModel.beatmapLevel.levelID, lobbyPlayerDataModel.beatmapCharacteristic.serializedName, lobbyPlayerDataModel.beatmapDifficulty));
            }
        }

        /// <summary>
        /// Triggered when a player selects a song using a vanilla packet.
        /// </summary>
        public async override void HandleMenuRpcManagerRecommendBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            OnSelectedBeatmap(userId, beatmapId);
            string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
            if (hash != null)
            {
                Plugin.Log?.Debug($"'{userId}' selected song '{beatmapId.levelID}'.");
                BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                if (_playersData.Values.Any(playerData => playerData.beatmapLevel?.levelID == beatmapId.levelID))
                {
                    PreviewBeatmapStub? preview = GetExistingPreview(beatmapId.levelID);
                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic));
                }
                else
                {
                    PreviewBeatmapStub? preview = null;
                    IPreviewBeatmapLevel? localPreview = SongCore.Loader.GetLevelById(beatmapId.levelID);
                    if (localPreview != null)
                        preview = new PreviewBeatmapStub(hash, localPreview);
                    if (preview == null)
                        preview = await FetchBeatSaverPreview(beatmapId.levelID, hash);
                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic));
                }
            }
            else
                base.HandleMenuRpcManagerRecommendBeatmap(userId, beatmapId);
        }

        /// <summary>
        /// Triggered when the local player selects a song.
        /// </summary>
        public async new void SetLocalPlayerBeatmapLevel(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            string? hash = Utilities.Utils.LevelIdToHash(levelId);
            Plugin.Log?.Debug($"Local user selected song '{hash ?? levelId}'.");
            if (hash != null)
            {
                if (_playersData.Values.Any(playerData => playerData.beatmapLevel?.levelID == levelId))
                {
                    PreviewBeatmapStub? preview = GetExistingPreview(levelId);
                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic));
                    _menuRpcManager.RecommendBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                }
                else
                {
                    PreviewBeatmapStub? preview = null;
                    IPreviewBeatmapLevel? localPreview = SongCore.Loader.GetLevelById(levelId);
                    if (localPreview != null)
                        preview = new PreviewBeatmapStub(hash, localPreview);
                    if (preview == null)
                        preview = await FetchBeatSaverPreview(levelId, hash);

                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic));
                    _packetManager.Send(new PreviewBeatmapPacket(preview!, characteristic.serializedName, beatmapDifficulty));
                    _menuRpcManager.RecommendBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                }
            }else
                base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }

		public override void HandleMenuRpcManagerSetPlayersPermissionConfiguration(string userId, PlayersLobbyPermissionConfigurationNetSerializable permissionConfiguration)
		{
            foreach (PlayerLobbyPermissionConfigurationNetSerializable playerPermission in permissionConfiguration.playersPermission)
            {
                Plugin.Log?.Info($"Processing PermissionConfiguration for '{playerPermission.userId}'");

                ExtendedPlayer? player = _sessionManager.GetExtendedPlayer(playerPermission.userId);
                if (player != null)
				{
                    player.isPartyOwner = playerPermission.isServerOwner;
                    player.hasRecommendBeatmapPermission = playerPermission.hasRecommendBeatmapsPermission;
                    player.hasRecommendModifiersPermission = playerPermission.hasRecommendGameplayModifiersPermission;
                    player.hasKickVotePermission = playerPermission.hasKickVotePermission;
                }

                // Base game is bugged and this method is fucked
                // this.SetPlayerIsPartyOwner(playerPermission.userId, playerPermission.isServerOwner, true);
                ILobbyPlayerData lobbyPlayerData = this.GetOrCreateLobbyPlayerDataModel(playerPermission.userId, out bool alreadyExists);
                if (playerPermission.isServerOwner)
                    this.SetPlayerIsPartyOwner(playerPermission.userId, playerPermission.isServerOwner, true);
                else if (lobbyPlayerData.isPartyOwner != playerPermission.isServerOwner)
				{
                    lobbyPlayerData.isPartyOwner = playerPermission.isServerOwner;
                    this.NotifyModelChange(playerPermission.userId);
				}
            }

            MPState.LocalPlayerIsHost = localUserId == partyOwnerId;
            _sessionManager.partyOwner = _sessionManager.GetPlayerByUserId(partyOwnerId);
        }

		/// <summary>
		/// Used to raise the <see cref="MultiplayerExtensions.MPEvents.BeatmapSelected"/> event.
		/// </summary>
		private void OnSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable? beatmapId)
        {
            SelectedBeatmapEventArgs args;
            UserType userType = UserType.None;
            IConnectedPlayer? player = _multiplayerSessionManager.GetPlayerByUserId(userId);
            if (player != null)
            {
                if (player.isMe)
                    userType |= UserType.Local;
                if (player.isConnectionOwner)
                    userType |= UserType.Host;
            }
            else
                Plugin.Log.Warn($"OnSelectedBeatmap raised by an unknown player: {userId}. Selected '{beatmapId?.levelID ?? "<NULL>"}'");
            if (beatmapId == null || string.IsNullOrEmpty(beatmapId.levelID))
            {
                args = new SelectedBeatmapEventArgs(userId, userType);
            }
            else
            {
                BeatmapCharacteristicSO? characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                if (characteristic == null)
                    Plugin.Log?.Warn($"Unknown characteristic: '{beatmapId.beatmapCharacteristicSerializedName}'");
                args = new SelectedBeatmapEventArgs(userId, userType, beatmapId.levelID, beatmapId.difficulty, characteristic);
            }
            MPEvents.RaiseBeatmapSelected(this, args);
        }

        /// <summary>
        /// Grabs a preview from a song another player already has selected.
        /// </summary>
        public PreviewBeatmapStub? GetExistingPreview(string levelID)
        {
            IPreviewBeatmapLevel? preview = _playersData.Values.ToList().Find(playerData => playerData.beatmapLevel?.levelID == levelID)?.beatmapLevel;
            if (preview is PreviewBeatmapStub previewBeatmap)
                return previewBeatmap;
            return null;
        }

        /// <summary>
        /// Creates a preview from a BeatSaver request.
        /// </summary>
        public async Task<PreviewBeatmapStub?> FetchBeatSaverPreview(string levelID, string hash)
        {
            try
            {
                Beatmap? bm = await Plugin.BeatSaver.BeatmapByHash(hash);
                if (bm != null)
                    return new PreviewBeatmapStub(levelID, hash, bm);
                return null;
            }
            catch(Exception ex)
            {
                Plugin.Log.Error(ex.Message);
                return null;
            }
        }
    }
}
