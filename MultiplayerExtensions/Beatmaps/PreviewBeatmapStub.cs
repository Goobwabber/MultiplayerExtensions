using BeatSaverSharp.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Beatmaps
{
    public class PreviewBeatmapStub : IPreviewBeatmapLevel
    {
        private Beatmap? _beatmap { get; set; }
        private IPreviewBeatmapLevel? _preview { get; set; }
        private DownloadableState _downloadable = DownloadableState.Unchecked;
        private Task<bool>? _downloadableTask;

        public bool isDownloaded { get; private set; }
        public Task<bool> isDownloadable
        {
            get
            {
                if (_downloadable != DownloadableState.Unchecked)
                    return Task.FromResult(_downloadable == DownloadableState.True);

                if (_downloadableTask == null)
				{
                    _downloadableTask = Plugin.BeatSaver.BeatmapByHash(levelHash).ContinueWith<bool>(r => r.Exception == null && r.Result is Beatmap);
                    _downloadableTask.ContinueWith(r => _downloadable = r.Result ? DownloadableState.True : DownloadableState.False);
				}

                return _downloadableTask!;
            }
        }



        public string levelID { get; private set; }
        public string levelHash { get; private set; }

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



        public PreviewBeatmapStub(string levelHash, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            this._preview = previewBeatmapLevel;
            this.isDownloaded = true;

            this.levelID = this._preview.levelID;
            this.levelHash = levelHash;

            this.songName = this._preview.songName;
            this.songSubName = this._preview.songSubName;
            this.songAuthorName = this._preview.songAuthorName;
            this.levelAuthorName = this._preview.levelAuthorName;
            this.beatsPerMinute = this._preview.beatsPerMinute;
            this.songDuration = this._preview.songDuration;
        }

        public PreviewBeatmapStub(PreviewBeatmapPacket packet)
        {
            this.isDownloaded = false;

            this.levelID = packet.levelId;
            this.levelHash = packet.levelHash;

            this.songName = packet.songName;
            this.songSubName = packet.songSubName;
            this.songAuthorName = packet.songAuthorName;
            this.levelAuthorName = packet.levelAuthorName;
            this.beatsPerMinute = packet.beatsPerMinute;
            this.songDuration = packet.songDuration;
        }

        public PreviewBeatmapStub(string levelID, string hash, Beatmap bm)
        {
            this._beatmap = bm;
            this._downloadable = DownloadableState.True;
            this.isDownloaded = false;

            this.levelID = levelID;
            this.levelHash = hash;

            this.songName = bm.Metadata.SongName;
            this.songSubName = bm.Metadata.SongSubName;
            this.songAuthorName = bm.Metadata.SongAuthorName;
            this.levelAuthorName = bm.Metadata.LevelAuthorName;
            this.beatsPerMinute = bm.Metadata.BPM;
            this.songDuration = bm.Metadata.Duration;
        }



        public async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            if (_preview != null)
                return await _preview.GetCoverImageAsync(cancellationToken);

            if (_beatmap != null)
			{
                try
                {
                    Sprite? cover = Utilities.Sprites.GetSprite(await _beatmap.Versions[0].DownloadCoverImage());
                    if (cover != null)
                        return cover;
				}
				catch(Exception ex)
				{
                    Plugin.Log?.Warn($"Failed to fetch beatmap cover: {ex.Message}");
				}
			}

            return Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 2, 2), new Vector2(0, 0), 100.0f);
        }

        public Task<AudioClip>? GetPreviewAudioClipAsync(CancellationToken cancellationToken) => _preview?.GetPreviewAudioClipAsync(cancellationToken);

        private enum DownloadableState
        {
            True, False, Unchecked
        }
    }
}
