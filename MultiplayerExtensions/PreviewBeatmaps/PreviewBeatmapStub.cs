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
        public bool isDownloaded { get; private set; }
        public bool isDownloadable { get; private set; }
        public string? downloadURL { get; private set; }

        private IPreviewBeatmapLevel _localPreview;
        private Task<Beatmap?>? _fetchBeatmap;
        public Beatmap? beatmap
        {
            get
            {
                if (_fetchBeatmap != null && _fetchBeatmap.IsCompleted)
                {
                    return _fetchBeatmap?.Result;
                }
                return null;
            }
        }

        public PreviewBeatmapStub(string levelID)
        {
            this.levelID = levelID;
            this.levelHash = Utilities.Utils.LevelIdToHash(levelID)!;
            _localPreview = SongCore.Loader.GetLevelById(levelID);
            Populate(_localPreview);
        }

        public PreviewBeatmapStub(string levelID, string songName, string songSubName, string songAuthorName, string levelAuthorName) : this(levelID)
        {
            this.songName = songName;
            this.songSubName = songSubName;
            this.songAuthorName = songAuthorName;
            this.levelAuthorName = levelAuthorName;
        }

        public async Task<PreviewBeatmapStub> FetchPopulated()
        {
            await FetchBeatmap();
            return this;
        }

        private object _fetchLock = new object();
        public Task<Beatmap?> FetchBeatmap()
        {
            if (levelHash == null || levelHash.Length == 0)
            {
                Plugin.Log?.Warn($"Beatmap with level ID '{levelID}' cannot be converted to a valid Beat Saver hash.");
                return Task.FromResult<Beatmap?>(null);
            }
            lock (_fetchLock)
            {
                if (_fetchBeatmap == null)
                {
                    _fetchBeatmap = GetBeatmap(levelHash);
                    _fetchBeatmap.ContinueWith(r =>
                    {
                        Populate(r.Result);
                    });
                }
            }
            return _fetchBeatmap;
        }

        private async Task<Beatmap?> GetBeatmap(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return null;
            Task<Beatmap?> beatmap = BeatSaver.Client.Hash(hash);
            await beatmap.ContinueWith(r =>
            {
                if (r.IsCanceled)
                {
                    Plugin.Log?.Debug($"PreviewBeatmap({levelID}): Metadata retrieval canceled.");
                    return;
                }
                else if (r.IsFaulted)
                {
                    Plugin.Log?.Error($"PreviewBeatmap({levelID}): Error retrieving metadata: {r.Exception.Message}");
                    Plugin.Log?.Debug(r.Exception);
                    return;
                }
            });

            return await beatmap;
        }

        public PreviewBeatmapStub Populate(Beatmap? bm)
        {
            if (bm != null)
            {
                isDownloadable = true;
                Plugin.Log?.Debug($"PreviewBeatmap({levelID}): Metadata downloaded.");
                downloadURL = bm.DownloadURL;
                songName ??= bm.Metadata.SongName;
                songSubName ??= bm.Metadata.SongSubName;
                songAuthorName ??= bm.Metadata.SongAuthorName;
                levelAuthorName ??= bm.Metadata.LevelAuthorName;
                beatsPerMinute = bm.Metadata.BPM;
                songDuration = bm.Metadata.Duration;
            }

            return this;
        }

        public PreviewBeatmapStub Populate(IPreviewBeatmapLevel bm)
        {
            if (bm != null)
            {
                isDownloaded = true;
                Plugin.Log?.Debug($"PreviewBeatmap({levelID}): Loaded from local.");
                songName ??= bm.songName;
                songSubName ??= bm.songSubName;
                songAuthorName ??= bm.songAuthorName;
                levelAuthorName ??= bm.levelAuthorName;
                beatsPerMinute = bm.beatsPerMinute;
                songDuration = bm.songDuration;
            }

            return this;
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

        public async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            if (_localPreview != null)
            {
                return await _localPreview.GetCoverImageAsync(cancellationToken);
            }

            Beatmap? bm = await FetchBeatmap();
            Sprite? sprite = null;
            if (bm != null && await bm.FetchCoverImage(cancellationToken) is byte[] img)
            {
                sprite = Utilities.Utils.GetSprite(img);
            }
            if (sprite == null)
                sprite = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 2, 2), new Vector2(0, 0), 100.0f);
            return sprite;
        }

        public Task<AudioClip?> GetPreviewAudioClipAsync(CancellationToken cancellationToken)
        {
            if (_localPreview != null)
            {
                return _localPreview.GetPreviewAudioClipAsync(cancellationToken);
            }
            return Task.FromResult<AudioClip?>(null);
        }
    }
}
