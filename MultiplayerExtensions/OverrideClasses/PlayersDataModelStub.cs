using BeatSaverSharp;
using MultiplayerExtensions.Beatmaps;
using MultiplayerExtensions.Networking;
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
        protected readonly BeatmapLevelsModel _beatmapLevelsModel;

        [Inject]
        protected readonly BeatmapCharacteristicCollectionSO _beatmapCharacteristicCollection;

        [Inject]
        protected readonly IMenuRpcManager _menuRpcManager;

        [Inject]
        protected readonly ExtendedSessionManager _sessionManager;

        public PlayersDataModelStub() { }

        public new void Activate()
        {
            _sessionManager.RegisterCallback(ExtendedSessionManager.MessageType.PreviewBeatmapUpdate, HandlePreviewBeatmapPacket, new Func<PreviewBeatmapPacket>(PreviewBeatmapPacket.pool.Obtain));
            _sessionManager.playerStateChangedEvent += HandlePlayerStateChanged;
            base.Activate();

            _menuRpcManager.selectedBeatmapEvent -= base.HandleMenuRpcManagerSelectedBeatmap;
            _menuRpcManager.selectedBeatmapEvent += this.HandleMenuRpcManagerSelectedBeatmap;
        }

        private void HandlePlayerStateChanged(ExtendedPlayer player)
        {
            HarmonyPatches.GameServerPlayerTableColor.UpdateColor(player);
        }

        public void HandlePreviewBeatmapPacket(PreviewBeatmapPacket packet, ExtendedPlayer player)
        {
            if (Utilities.Utils.LevelIdToHash(packet.levelId) != null)
            {
                PreviewBeatmapStub preview = PreviewBeatmapManager.GetPreview(packet);
                if (!preview.isDownloaded)
                {
                    BeatmapCharacteristicSO? characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(packet.characteristic);
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        base.SetPlayerBeatmapLevel(player.userId, preview, packet.difficulty, characteristic);
                    });
                }
            }
        }

        public void SendPreviewBeatmapPacket(PreviewBeatmapStub preview, string characteristic, BeatmapDifficulty difficulty)
        {
            PreviewBeatmapPacket beatmapPacket = new PreviewBeatmapPacket().FromPreview(preview, characteristic, difficulty);
            Plugin.Log?.Info($"Sending 'PreviewBeatmapPacket' with {preview.levelID}");
            _sessionManager.Send(beatmapPacket);
        }

        public override void HandleMenuRpcManagerSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            if (beatmapId != null)
            {
                string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
                if (hash != null)
                {
                    Plugin.Log?.Debug($"'{userId}' selected song '{hash}'.");
                    BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);
                    PreviewBeatmapManager.GetPopulatedPreview(beatmapId.levelID).ContinueWith(r =>
                    {
                        PreviewBeatmapStub preview = r.Result;

                        Plugin.Log?.Info($"user: {userId} | hostuser: {base.hostUserId}");
                        Plugin.Log?.Info($"local: {preview.isDownloaded} | cloud: {preview.isDownloadable}");

                        if (userId == base.hostUserId)
                        {
                            _sessionManager.SetLocalPlayerState("bmlocal", preview.isDownloaded);
                            _sessionManager.SetLocalPlayerState("bmcloud", preview.isDownloadable);
                        }

                        HMMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            base.SetPlayerBeatmapLevel(userId, preview, beatmapId.difficulty, characteristic);
                        });
                    });

                    return;
                }
            }

            base.HandleMenuRpcManagerSelectedBeatmap(userId, beatmapId);
        }

        public new void SetLocalPlayerBeatmapLevel(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            string? hash = Utilities.Utils.LevelIdToHash(levelId);
            if (hash != null)
            {
                Plugin.Log?.Debug($"Local user selected song '{hash}'.");
                PreviewBeatmapManager.GetPopulatedPreview(levelId).ContinueWith(r =>
                {
                    PreviewBeatmapStub preview = r.Result;

                    Plugin.Log?.Info($"localuser: {base.localUserId} | hostuser: {base.hostUserId}");
                    Plugin.Log?.Info($"local: {preview.isDownloaded} | cloud: {preview.isDownloadable}");

                    if (base.localUserId == base.hostUserId)
                    {
                        _sessionManager.SetLocalPlayerState("bmlocal", preview.isDownloaded);
                        _sessionManager.SetLocalPlayerState("bmcloud", preview.isDownloadable);
                    }

                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        SendPreviewBeatmapPacket(preview, characteristic.serializedName, beatmapDifficulty);
                        _menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                        base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic);
                    });
                });
                return;
            }

            base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }
    }
}
