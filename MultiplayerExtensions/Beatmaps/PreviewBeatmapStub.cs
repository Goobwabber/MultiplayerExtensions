using BeatSaverSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Beatmaps
{
    class PreviewBeatmapStub : IPreviewBeatmapLevel
    {
        public string levelHash { get; private set; }
        public string downloadURL => $"https://beatsaver.com/api/download/hash/{levelHash.ToLower()}";
        public Beatmap? beatmap;

        private Task<Sprite?>? _coverTask;
        private Task<byte[]> _rawCoverTask;
        private Task<AudioClip>? _audioTask;

        public bool isDownloaded;

        private enum DownloadableState
        {
            True, False, Unchecked
        }

        private DownloadableState _downloadable = DownloadableState.Unchecked;
        private Task<bool>? _downloadableTask;
        public Task<bool> isDownloadable
        {
            get
            {
                if (_downloadableTask == null)
                {
                    _downloadableTask = _downloadable != DownloadableState.Unchecked ?
                        new Task<bool>(() => _downloadable == DownloadableState.True) :
                        Plugin.BeatSaver.Hash(levelHash)
                        .ContinueWith<bool>(r =>
                        {
                            try
                            {
                                beatmap = r.Result;
                                _downloadable = beatmap is Beatmap ? DownloadableState.True : DownloadableState.False;
                                return _downloadable == DownloadableState.True;
                            }
                            catch
                            {
                                Plugin.Log.Warn($"Beat Saver request for song '{levelHash}' failed.");
                                _downloadable = DownloadableState.False;
                                return _downloadable == DownloadableState.True;
                            }
                        });
                }

                return _downloadableTask!;
            }
        }

        public PreviewBeatmapStub(string levelHash, IPreviewBeatmapLevel preview)
        {
            this.levelID = preview.levelID;
            this.levelHash = levelHash;
            this.isDownloaded = true;

            this.songName = preview.songName;
            this.songSubName = preview.songSubName;
            this.songAuthorName = preview.songAuthorName;
            this.levelAuthorName = preview.levelAuthorName;

            this.beatsPerMinute = preview.beatsPerMinute;
            this.songDuration = preview.songDuration;

            _coverTask = preview.GetCoverImageAsync(CancellationToken.None);
            _rawCoverTask = GetCoverImageAsync(CancellationToken.None).ContinueWith<byte[]>(task => Utilities.Sprites.GetRaw(task.Result));
            _audioTask = preview.GetPreviewAudioClipAsync(CancellationToken.None);
        }

        public PreviewBeatmapStub(PreviewBeatmapPacket packet)
        {
            this.levelID = packet.levelId;
            this.levelHash = Utilities.Utils.LevelIdToHash(levelID)!;
            this.isDownloaded = false;

            this.songName = packet.songName;
            this.songSubName = packet.songSubName;
            this.songAuthorName = packet.songAuthorName;
            this.levelAuthorName = packet.levelAuthorName;

            this.beatsPerMinute = packet.beatsPerMinute;
            this.songDuration = packet.songDuration;

            _rawCoverTask = Task.FromResult(packet.coverImage);
        }

        public PreviewBeatmapStub(string levelID, Beatmap bm)
        {
            this.levelID = levelID;
            this.levelHash = bm.Hash;
            
            this.beatmap = bm;
            this.isDownloaded = false;

            this.songName = bm.Metadata.SongName;
            this.songSubName = bm.Metadata.SongSubName;
            this.songAuthorName = bm.Metadata.SongAuthorName;
            this.levelAuthorName = bm.Metadata.LevelAuthorName;

            this.beatsPerMinute = bm.Metadata.BPM;
            this.songDuration = bm.Metadata.Duration;

            this._downloadable = DownloadableState.True;

            _rawCoverTask = bm.CoverImageBytes();
        }

        public string levelID { get; private set; }
        public string songName { get; private set; }
        public string songSubName { get; private set; }
        public string songAuthorName { get; private set; }
        public string levelAuthorName { get; private set; }
        public float beatsPerMinute { get; private set; }
        public float songDuration { get; private set; }
        public float songTimeOffset { get; private set; }
        public float shuffle { get; private set; }
        public float shufflePeriod { get; private set; }
        public float previewStartTime { get; private set; }
        public float previewDuration { get; private set; }
        public EnvironmentInfoSO? environmentInfo { get; private set; }
        public EnvironmentInfoSO? allDirectionsEnvironmentInfo { get; private set; }
        public PreviewDifficultyBeatmapSet[]? previewDifficultyBeatmapSets { get; private set; }

        public async Task<byte[]?> DownloadZip(CancellationToken cancellationToken, IProgress<double>? progress = null)
        {
            if (beatmap == null)
            {
                try
                {
                    beatmap = await Plugin.BeatSaver.Hash(levelHash);
                }
                catch
                {
                    Plugin.Log?.Warn($"Song '{levelHash}' cannot be downloaded form Beat Saver.");
                    return null;
                }
            }
            return await beatmap.ZipBytes(false);
        }

        public Task<byte[]> GetRawCoverAsync(CancellationToken cancellationToken) => _rawCoverTask;
        public Task<AudioClip>? GetPreviewAudioClipAsync(CancellationToken cancellationToken) => _audioTask;

        public async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            Sprite? cover = null;
            if (_coverTask != null)
                cover = await _coverTask;
            else
                Utilities.Sprites.GetSprite(await _rawCoverTask);

            if (cover == null)
                cover = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 2, 2), new Vector2(0, 0), 100.0f);

            return cover;
        }
    }
}
