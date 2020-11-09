using BeatSaverSharp;
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

        private Dictionary<string, PreviewBeatmapLevelStub> beatmapPreviews = new Dictionary<string, PreviewBeatmapLevelStub>();

        public PlayersDataModelStub() { }

        public new void Activate()
        {
            _sessionManager.RegisterCallback(ExtendedSessionManager.MessageType.PreviewBeatmapUpdate, HandlePreviewBeatmapPacket, new Func<PreviewBeatmapPacket>(PreviewBeatmapPacket.pool.Obtain));
            base.Activate();
        }

        public void HandlePreviewBeatmapPacket(PreviewBeatmapPacket packet, ExtendedPlayer player)
        {
            if (isCustomSong(packet.levelId, out string hash) && SongCore.Loader.GetLevelById(packet.levelId) == null)
            {
                BeatmapCharacteristicSO? characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(packet.characteristic);
                beatmapPreviews.Add(packet.levelId, packet.getPreviewBeatmap());
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    base.SetPlayerBeatmapLevel(player.userId, packet.getPreviewBeatmap(), packet.difficulty, characteristic);
                });
            }
        }

        public void SendPreviewBeatmapPacket(string levelId, string songName, string songSubName, string songAuthorName, string levelAuthorName, string characteristic, BeatmapDifficulty difficulty)
        {
            PreviewBeatmapPacket beatmapPacket = new PreviewBeatmapPacket().Init(levelId, songName, songSubName, songAuthorName, levelAuthorName, characteristic, difficulty);
            SendPreviewBeatmapPacket(beatmapPacket);
        }

        public void SendPreviewBeatmapPacket(PreviewBeatmapPacket packet)
        {
            Plugin.Log?.Info($"Sending 'PreviewBeatmapPacket' with {packet.levelId}");
            _sessionManager.Send(packet);
        }

        public override void HandleMenuRpcManagerSelectedBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            if (beatmapId != null)
            {
                if (isCustomSong(beatmapId.levelID, out string hash))
                {
                    Plugin.Log?.Debug($"'{userId}' selected song '{hash}'.");
                    if (SongCore.Loader.GetLevelById(beatmapId.levelID) != null)
                    {
                        base.HandleMenuRpcManagerSelectedBeatmap(userId, beatmapId);
                        Plugin.Log?.Debug($"Custom song '{hash}' loaded.");
                        return;
                    }

                    BeatmapCharacteristicSO? characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(beatmapId.beatmapCharacteristicSerializedName);

                    if (beatmapPreviews.TryGetValue(beatmapId.levelID, out PreviewBeatmapLevelStub previewBeatmap))
                    {
                        Plugin.Log?.Debug("PreviewBeatmapLevel found, skipping beatsaver request.");
                        base.SetPlayerBeatmapLevel(userId, previewBeatmap, beatmapId.difficulty, characteristic);
                        return;
                    }

                    GetBeatmap(hash)?.ContinueWith(r =>
                    {
                        PreviewBeatmapLevelStub preview = new PreviewBeatmapLevelStub(beatmapId.levelID, r.Result);
                        beatmapPreviews.Add(beatmapId.levelID, preview);
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
            if (isCustomSong(levelId, out string hash))
            {
                Plugin.Log?.Debug($"Local user selected song '{hash}'.");
                if (SongCore.Loader.GetLevelById(levelId) != null)
                {
                    IPreviewBeatmapLevel preview = _beatmapLevelsModel.GetLevelPreviewForLevelId(levelId);
                    SendPreviewBeatmapPacket(levelId, preview.songName, preview.songSubName, preview.songAuthorName, preview.levelAuthorName, characteristic.serializedName, beatmapDifficulty);

                    base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
                    Plugin.Log?.Debug($"Custom song '{hash}' loaded.");
                    return;
                }

                if (beatmapPreviews.TryGetValue(levelId, out PreviewBeatmapLevelStub previewBeatmap))
                {
                    Plugin.Log?.Debug("PreviewBeatmapLevel found, skipping beatsaver request.");
                    SendPreviewBeatmapPacket(new PreviewBeatmapPacket().FromPreview(previewBeatmap, characteristic.serializedName, beatmapDifficulty));
                    _menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                    base.SetPlayerBeatmapLevel(base.localUserId, previewBeatmap, beatmapDifficulty, characteristic);
                    return;
                }

                GetBeatmap(hash)?.ContinueWith(r =>
                {
                    PreviewBeatmapLevelStub preview = new PreviewBeatmapLevelStub(levelId, r.Result);
                    beatmapPreviews.Add(levelId, preview);
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        SendPreviewBeatmapPacket(new PreviewBeatmapPacket().FromPreview(preview, characteristic.serializedName, beatmapDifficulty));
                        _menuRpcManager.SelectBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                        base.SetPlayerBeatmapLevel(base.localUserId, preview, beatmapDifficulty, characteristic);
                    });
                });

                return;
            }

            base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }

        private async Task<Beatmap>? GetBeatmap(string? hash)
        {
            Task<Beatmap>? beatmap = BeatSaver.Client.Hash(hash);
            await beatmap.ContinueWith(r =>
            {
                if (r.IsCanceled)
                {
                    Plugin.Log?.Debug($"Metadata retrieval for {hash} was canceled.");
                    return;
                }
                else if (r.IsFaulted)
                {
                    Plugin.Log?.Error($"Error retrieving metadata for {hash}: {r.Exception.Message}");
                    Plugin.Log?.Debug(r.Exception);
                    return;
                }
            });

            return await beatmap;
        }

        private bool isCustomSong(string levelId, out string hash)
        {
            hash = Utilities.Utils.LevelIdToHash(levelId);
            return hash != null;
        }
    }
}
