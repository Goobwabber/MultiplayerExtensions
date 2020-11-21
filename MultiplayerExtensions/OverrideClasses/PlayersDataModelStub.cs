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

        private void HandlePlayerStateChanged(IConnectedPlayer player) => HarmonyPatches.GameServerPlayerTablePatch.Update(player);

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            if (localBeatmap != null)
            {
                _packetManager.Send(localBeatmap);
            }
        }

        public void HandlePreviewBeatmapPacket(PreviewBeatmapPacket packet, IConnectedPlayer player)
        {
            if (Utilities.Utils.LevelIdToHash(packet.levelId) != null)
            {
                PreviewBeatmapStub preview = PreviewBeatmapManager.CreatePreview(packet);
                BeatmapCharacteristicSO? characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(packet.characteristic);
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    base.SetPlayerBeatmapLevel(player.userId, preview, packet.difficulty, characteristic);
                });
            }
        }

        public async override void HandleMenuRpcManagerSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            if (!_sessionManager.GetPlayerByUserId(userId).HasState("modded"))
            {
                if (beatmapId != null)
                {
                    string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
                    if (hash != null)
                    {
                        Plugin.Log?.Debug($"'{userId}' selected song '{hash}'.");
                        BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                        PreviewBeatmapStub preview = await PreviewBeatmapManager.CreatePreview(beatmapId.levelID);

                        if (userId == base.hostUserId)
                            _sessionManager.SetLocalPlayerState("beatmap_downloaded", preview.isDownloaded);

                        HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic));
                        return;
                    }
                }

                base.HandleMenuRpcManagerSelectedBeatmap(userId, beatmapId);
            }
        }

        public async new void SetLocalPlayerBeatmapLevel(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            string? hash = Utilities.Utils.LevelIdToHash(levelId);
            if (hash != null)
            {
                Plugin.Log?.Debug($"Local user selected song '{hash}'.");
                //HarmonyPatches.GameServerPlayerTablePatch.SetLoading(_sessionManager.localPlayer);
                PreviewBeatmapStub preview = await PreviewBeatmapManager.CreatePreview(levelId);

                if (base.localUserId == base.hostUserId)
                    _sessionManager.SetLocalPlayerState("beatmap_downloaded", preview.isDownloaded);

                _packetManager.Send(await PreviewBeatmapPacket.FromPreview(preview, characteristic.serializedName, beatmapDifficulty));
                _menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                HMMainThreadDispatcher.instance.Enqueue(() => base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic));
                return;
            }

            base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }
    }
}
