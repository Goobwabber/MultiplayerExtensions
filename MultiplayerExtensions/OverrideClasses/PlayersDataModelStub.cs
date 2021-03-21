using BeatSaverSharp;
using MultiplayerExtensions.Beatmaps;
using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Sessions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiplayerExtensions.OverrideClasses
{
    class PlayersDataModelStub : LobbyPlayersDataModel, ILobbyPlayersDataModel, IDisposable
    {
        protected readonly PacketManager _packetManager;
        protected readonly ExtendedPlayerManager _playerManager;

        internal PlayersDataModelStub(PacketManager packetManager, ExtendedPlayerManager playerManager)
        {
            _packetManager = packetManager;
            _playerManager = playerManager;
        }

        public new void Activate()
        {
            MPEvents.CustomSongsChanged += HandleCustomSongsChanged;
            MPEvents.FreeModChanged += HandleFreeModChanged;
            _packetManager.RegisterCallback<PreviewBeatmapPacket>(HandlePreviewBeatmapPacket);
            base.Activate();

            _menuRpcManager.selectedBeatmapEvent -= base.HandleMenuRpcManagerSelectedBeatmap;
            _menuRpcManager.selectedBeatmapEvent += HandleMenuRpcManagerSelectedBeatmap;
            _menuRpcManager.getSelectedBeatmapEvent -= base.HandleMenuRpcManagerGetSelectedBeatmap;
            _menuRpcManager.getSelectedBeatmapEvent += HandleMenuRpcManagerGetSelectedBeatmap;
            _menuRpcManager.clearSelectedBeatmapEvent -= base.HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.clearSelectedBeatmapEvent += HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.selectedGameplayModifiersEvent -= base.HandleMenuRpcManagerSelectedGameplayModifiers;
            _menuRpcManager.selectedGameplayModifiersEvent += HandleMenuRpcManagerSelectedGameplayModifiers;
            _menuRpcManager.clearSelectedGameplayModifiersEvent -= base.HandleMenuRpcManagerClearSelectedGameplayModifiers;
            _menuRpcManager.clearSelectedGameplayModifiersEvent += HandleMenuRpcManagerClearSelectedGameplayModifiers;
        }

        public new void Deactivate()
        {
            MPEvents.CustomSongsChanged -= HandleCustomSongsChanged;
            MPEvents.FreeModChanged -= HandleFreeModChanged;
            _packetManager.UnregisterCallback<PreviewBeatmapPacket>();

            _menuRpcManager.selectedBeatmapEvent -= HandleMenuRpcManagerSelectedBeatmap;
            _menuRpcManager.selectedBeatmapEvent += base.HandleMenuRpcManagerSelectedBeatmap;
            _menuRpcManager.getSelectedBeatmapEvent -= HandleMenuRpcManagerGetSelectedBeatmap;
            _menuRpcManager.getSelectedBeatmapEvent += base.HandleMenuRpcManagerGetSelectedBeatmap;
            _menuRpcManager.clearSelectedBeatmapEvent -= HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.clearSelectedBeatmapEvent += base.HandleMenuRpcManagerClearBeatmap;
            _menuRpcManager.selectedGameplayModifiersEvent -= HandleMenuRpcManagerSelectedGameplayModifiers;
            _menuRpcManager.selectedGameplayModifiersEvent += base.HandleMenuRpcManagerSelectedGameplayModifiers;
            _menuRpcManager.clearSelectedGameplayModifiersEvent -= HandleMenuRpcManagerClearSelectedGameplayModifiers;
            _menuRpcManager.clearSelectedGameplayModifiersEvent += base.HandleMenuRpcManagerClearSelectedGameplayModifiers;

            base.Deactivate();
        }

        public new void Dispose()
        {
            Deactivate();
        }

        private void HandleCustomSongsChanged(object sender, bool value)
        {
            if (!value && GetPlayerBeatmapLevel(localUserId) is PreviewBeatmapStub)
            {
                base.ClearLocalPlayerBeatmapLevel();
            }
        }

        private void HandleFreeModChanged(object sender, bool value)
        {
            if (value && localUserId != hostUserId)
            {
                GameplayModifiers localModifiers = GetPlayerGameplayModifiers(localUserId);
                GameplayModifiers hostModifiers = GetPlayerGameplayModifiers(hostUserId);
                if (localModifiers.songSpeed != hostModifiers.songSpeed)
                    base.SetLocalPlayerGameplayModifiers(localModifiers.CopyWith(songSpeed: hostModifiers.songSpeed));
            }
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
                HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(player.userId, preview, packet.difficulty, characteristic));
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
        public async override void HandleMenuRpcManagerGetSelectedBeatmap(string userId)
        {
            ILobbyPlayerDataModel lobbyPlayerDataModel = this.GetLobbyPlayerDataModel(this.localUserId);
            if (lobbyPlayerDataModel != null && MPState.CurrentGameType != MultiplayerGameType.QuickPlay && _multiplayerSessionManager.GetPlayerByUserId(userId).HasState("modded") && lobbyPlayerDataModel?.beatmapLevel is PreviewBeatmapStub preview)
                _packetManager.Send(await PreviewBeatmapPacket.FromPreview(preview, lobbyPlayerDataModel.beatmapCharacteristic.serializedName, lobbyPlayerDataModel.beatmapDifficulty));
            else if (lobbyPlayerDataModel != null && lobbyPlayerDataModel.beatmapLevel != null)
                this._menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(lobbyPlayerDataModel.beatmapLevel.levelID, lobbyPlayerDataModel.beatmapCharacteristic.serializedName, lobbyPlayerDataModel.beatmapDifficulty));
        }

        /// <summary>
        /// Triggered when a player selects a song using a vanilla packet.
        /// </summary>
        public async override void HandleMenuRpcManagerSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            OnSelectedBeatmap(userId, beatmapId);
            string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
            Plugin.Log?.Debug($"'{userId}' selected song '{hash ?? beatmapId.levelID}'.");
            if (hash != null)
            {
                BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                if (_playersData.Values.Any(playerData => playerData.beatmapLevel?.levelID == beatmapId.levelID))
                {
                    PreviewBeatmapStub? preview = GetExistingPreview(beatmapId.levelID);
                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic));
                }
                else
                {
                    PreviewBeatmapStub? preview = null;
                    IPreviewBeatmapLevel localPreview = SongCore.Loader.GetLevelById(beatmapId.levelID);
                    if (localPreview != null)
                        preview = new PreviewBeatmapStub(hash, localPreview);
                    if (preview == null)
                        preview = await FetchBeatSaverPreview(beatmapId.levelID, hash);
                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic));
                }
            }
            else
                base.HandleMenuRpcManagerSelectedBeatmap(userId, beatmapId);
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
                    _menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                }
                else
                {
                    PreviewBeatmapStub? preview = null;
                    IPreviewBeatmapLevel localPreview = SongCore.Loader.GetLevelById(levelId);
                    if (localPreview != null)
                        preview = new PreviewBeatmapStub(hash, localPreview);
                    if (preview == null)
                        preview = await FetchBeatSaverPreview(levelId, hash);

                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic));
                    _packetManager.Send(await PreviewBeatmapPacket.FromPreview(preview, characteristic.serializedName, beatmapDifficulty));
                    if (!_multiplayerSessionManager.connectedPlayers.All(x => x.HasState("modded")))
                        _menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                }
            }else
                base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }

        public override void HandleMenuRpcManagerSelectedGameplayModifiers(string userId, GameplayModifiers gameplayModifiers)
        {
            ExtendedPlayer? player = _playerManager.GetExtendedPlayer(userId);
            if (player != null)
                player.lastModifiers = gameplayModifiers;
            base.HandleMenuRpcManagerSelectedGameplayModifiers(userId, gameplayModifiers);
            if (userId == hostUserId && MPState.FreeModEnabled)
            {
                GameplayModifiers localModifiers = GetPlayerGameplayModifiers(localUserId);
                if (localModifiers.songSpeed != gameplayModifiers.songSpeed)
                    base.SetLocalPlayerGameplayModifiers(localModifiers.CopyWith(songSpeed: gameplayModifiers.songSpeed));
            }
        }

        public override void HandleMenuRpcManagerClearSelectedGameplayModifiers(string userId)
        {
            ExtendedPlayer? player = _playerManager.GetExtendedPlayer(userId);
            if (player != null)
                player.lastModifiers = null;
            base.HandleMenuRpcManagerClearSelectedGameplayModifiers(userId);
            if (userId == hostUserId && MPState.FreeModEnabled)
            {
                GameplayModifiers localModifiers = GetPlayerGameplayModifiers(localUserId);
                if (localModifiers.songSpeed != GameplayModifiers.SongSpeed.Normal)
                    base.SetLocalPlayerGameplayModifiers(localModifiers.CopyWith(songSpeed: GameplayModifiers.SongSpeed.Normal));
            }
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
                Beatmap bm = await Plugin.BeatSaver.Hash(hash);
                return new PreviewBeatmapStub(levelID, bm);
            }
            catch(Exception ex)
            {
                Plugin.Log.Error(ex.Message);
                return null;
            }
        }
    }
}
