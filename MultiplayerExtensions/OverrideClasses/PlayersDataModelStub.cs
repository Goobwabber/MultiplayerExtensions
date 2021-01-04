using BeatSaverSharp;
using MultiplayerExtensions.Beatmaps;
using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.OverrideClasses
{
    class PlayersDataModelStub : LobbyPlayersDataModel, ILobbyPlayersDataModel
    {
        [Inject]
        protected readonly SessionManager _sessionManager;

        [Inject]
        protected readonly PacketManager _packetManager;

        public PlayersDataModelStub() { }

        private PreviewBeatmapPacket localBeatmap;

        public new void Activate()
        {
            _packetManager.RegisterCallback<PreviewBeatmapPacket>(HandlePreviewBeatmapPacket);
            _sessionManager.playerStateChangedEvent += HandlePlayerStateChanged;
            _sessionManager.playerConnectedEvent += HandlePlayerConnected;
            base.Activate();

            _menuRpcManager.selectedBeatmapEvent -= base.HandleMenuRpcManagerSelectedBeatmap;
            _menuRpcManager.selectedBeatmapEvent += this.HandleMenuRpcManagerSelectedBeatmap;
        }

        private void HandlePlayerStateChanged(IConnectedPlayer player)
        {
            if (player.HasState("beatmap_downloaded"))
            {
                this.NotifyModelChange(player.userId);
            }
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            if (localBeatmap != null)
            {
                _packetManager.Send(localBeatmap);
            }
        }

        public void HandlePreviewBeatmapPacket(PreviewBeatmapPacket packet, IConnectedPlayer player)
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

        public async override void HandleMenuRpcManagerSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            if (beatmapId != null)
            {
                string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
                if (hash != null)
                {
                    Plugin.Log?.Debug($"'{userId}' selected song '{hash}'.");
                    BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                    PreviewBeatmapStub? preview = null;

                    if (_playersData.Values.Any(playerData => playerData.beatmapLevel?.levelID == beatmapId.levelID))
                    {
                        IPreviewBeatmapLevel playerPreview = _playersData.Values.Where(playerData => playerData.beatmapLevel?.levelID == beatmapId.levelID).First().beatmapLevel;
                        if (playerPreview is PreviewBeatmapStub playerPreviewStub)
                            preview = playerPreviewStub;
                    }

                    IPreviewBeatmapLevel localPreview = SongCore.Loader.GetLevelById(beatmapId.levelID);
                    if (localPreview != null)
                        preview = new PreviewBeatmapStub(hash, localPreview);

                    if (preview == null)
                    {
                        try
                        {
                            Beatmap bm = await Plugin.BeatSaver.Hash(hash);
                            preview = new PreviewBeatmapStub(bm);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    if (userId == base.hostUserId)
                        _sessionManager.SetLocalPlayerState("beatmap_downloaded", preview.isDownloaded);

                    HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic));
                    return;
                }
            }

            base.HandleMenuRpcManagerSelectedBeatmap(userId, beatmapId);
        }

        public async new void SetLocalPlayerBeatmapLevel(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            string? hash = Utilities.Utils.LevelIdToHash(levelId);
            if (hash != null)
            {
                Plugin.Log?.Debug($"Local user selected song '{hash}'.");
                PreviewBeatmapStub? preview = null;

                if (_playersData.Values.Any(playerData => playerData.beatmapLevel?.levelID == levelId))
                {
                    IPreviewBeatmapLevel playerPreview = _playersData.Values.Where(playerData => playerData.beatmapLevel?.levelID == levelId).First().beatmapLevel;
                    if (playerPreview is PreviewBeatmapStub playerPreviewStub)
                        preview = playerPreviewStub;
                }

                IPreviewBeatmapLevel localPreview = SongCore.Loader.GetLevelById(levelId);
                if (localPreview != null)
                    preview = new PreviewBeatmapStub(hash, localPreview);

                if (preview == null)
                {
                    try
                    {
                        Beatmap bm = await Plugin.BeatSaver.Hash(hash);
                        preview = new PreviewBeatmapStub(bm);
                    }
                    catch
                    {
                        return;
                    }
                }

                if (base.localUserId == base.hostUserId)
                    _sessionManager.SetLocalPlayerState("beatmap_downloaded", preview.isDownloaded);

                HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic));
                _packetManager.Send(await PreviewBeatmapPacket.FromPreview(preview, characteristic.serializedName, beatmapDifficulty));
            }else
                base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }
    }
}
