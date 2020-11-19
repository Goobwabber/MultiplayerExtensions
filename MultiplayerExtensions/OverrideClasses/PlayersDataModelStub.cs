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
            HarmonyPatches.GameServerPlayerTableColor.UpdateColor(player);
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
                    localBeatmap = new PreviewBeatmapPacket().FromPreview(preview, characteristic.serializedName, beatmapDifficulty);

                    if (base.localUserId == base.hostUserId)
                    {
                        _sessionManager.SetLocalPlayerState("bmlocal", preview.isDownloaded);
                        _sessionManager.SetLocalPlayerState("bmcloud", preview.isDownloadable);
                    }

                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        _packetManager.Send(localBeatmap);
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
