using BeatSaverSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Beatmaps
{
    class PreviewBeatmapStub : IPreviewBeatmapLevel
    {
        public string levelHash { get; private set; }
        public string downloadURL => $"https://beatsaver.com/api/download/hash/{levelHash.ToLower()}";
        private Func<CancellationToken, Task<Sprite?>> _coverGetter;
        private Func<CancellationToken, Task<AudioClip>>? _audioGetter;
        private Func<CancellationToken, Task<byte[]>> _rawCoverGetter;

        public bool isDownloaded;
        private bool? _downloadable = null;
        private Task<bool>? _downloadableTask;
        public Task<bool> isDownloadable
        {
            get
            {
                if (_downloadableTask == null)
                {
                    if (_downloadable != null)
                    {
                        _downloadableTask = new Task<bool>(() =>
                        {
                            return (bool)_downloadable!;
                        });
                    }
                    else
                    {
                        Task<Beatmap> bm = BeatSaver.Client.Hash(levelHash, CancellationToken.None);
                        _downloadableTask = bm.ContinueWith<bool>(r =>
                        {
                            _downloadable = r.Result is Beatmap;
                            return (bool)_downloadable!;
                        });
                    }
                }

                return _downloadableTask!;
            }
        }

        public PreviewBeatmapStub(string levelID)
        {
            this.levelID = levelID;
            this.levelHash = Utilities.Utils.LevelIdToHash(levelID)!;
            this.isDownloaded = true;

            IPreviewBeatmapLevel preview = SongCore.Loader.GetLevelById(levelID);
            this.songName = preview.songName;
            this.songSubName = preview.songSubName;
            this.songAuthorName = preview.songAuthorName;
            this.levelAuthorName = preview.levelAuthorName;

            this.beatsPerMinute = preview.beatsPerMinute;
            this.songDuration = preview.songDuration;

            _coverGetter = (CancellationToken cancellationToken) => preview.GetCoverImageAsync(cancellationToken);
            _audioGetter = (CancellationToken cancellationToken) => preview.GetPreviewAudioClipAsync(cancellationToken);
            _rawCoverGetter = async (CancellationToken cancellationToken) => Utilities.Sprites.GetRaw(await GetCoverImageAsync(cancellationToken));
        }

        public PreviewBeatmapStub(string levelID, bool downloadable) : this(levelID)
        {
            this._downloadable = downloadable;
        }

        public PreviewBeatmapStub(string levelID, Beatmap bm)
        {
            this.levelID = levelID;
            this.levelHash = Utilities.Utils.LevelIdToHash(levelID)!;
            this.isDownloaded = false;

            this.songName = bm.Metadata.SongName;
            this.songSubName = bm.Metadata.SongSubName;
            this.songAuthorName = bm.Metadata.SongAuthorName;
            this.levelAuthorName = bm.Metadata.LevelAuthorName;

            this.beatsPerMinute = bm.Metadata.BPM;
            this.songDuration = bm.Metadata.Duration;

            this._downloadable = true;

            _rawCoverGetter = async (CancellationToken cancellationToken) => await bm.FetchCoverImage(cancellationToken);
            _coverGetter = async (CancellationToken cancellationToken) => Utilities.Sprites.GetSprite(await GetRawCoverAsync(cancellationToken));
        }

        public PreviewBeatmapStub(PreviewBeatmapPacket packet)
        {
            this.levelID = packet.levelId;
            this.levelHash = Utilities.Utils.LevelIdToHash(levelID)!;
            this.isDownloaded = SongCore.Loader.GetLevelById(packet.levelId) != null;

            this.songName = packet.songName;
            this.songSubName = packet.songSubName;
            this.songAuthorName = packet.songAuthorName;
            this.levelAuthorName = packet.levelAuthorName;

            this.beatsPerMinute = packet.beatsPerMinute;
            this.songDuration = packet.songDuration;

            this._downloadable = packet.isDownloadable;

            this._rawCover = packet.coverImage;
            _coverGetter = async (CancellationToken cancellationToken) => Utilities.Sprites.GetSprite(await GetRawCoverAsync(cancellationToken));
        }

        public string levelID { get; private set; }
        public string? songName { get; private set; }
        public string? songSubName { get; private set; }
        public string? songAuthorName { get; private set; }
        public string? levelAuthorName { get; private set; }
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

        private byte[]? _rawCover = null;
        public async Task<byte[]> GetRawCoverAsync(CancellationToken cancellationToken)
        {
            if (_rawCover == null)
                _rawCover = await _rawCoverGetter.Invoke(cancellationToken);
            return _rawCover;
        }

        private Sprite? _coverImage = null;
        public async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            if (_coverImage == null)
                _coverImage = await _coverGetter.Invoke(cancellationToken);
            if (_coverImage == null)
                _coverImage = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 2, 2), new Vector2(0, 0), 100.0f);
            return _coverImage;
        }

        public Task<AudioClip>? GetPreviewAudioClipAsync(CancellationToken cancellationToken)
        {
            return _audioGetter?.Invoke(cancellationToken);
        }
    }
}
