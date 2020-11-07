using BeatSaverSharp;
using MultiplayerExtensions.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.OverrideClasses
{
    public class PreviewBeatmapLevelStub : IPreviewBeatmapLevel
    {
        private object _getlock = new object();
        private Task<Beatmap?>? _getBeatmap;
        public Task<Beatmap?> GetBeatmap
        {
            get
            {
                lock (_getlock)
                {
                    if (_getBeatmap == null)
                    {
                        _getBeatmap = BeatSaver.Client.Hash(Utilities.Utils.LevelIdToHash(levelID));
                        _getBeatmap.ContinueWith(b =>
                        {
                            Populate(b.Result);
                        });
                    }
                }
                return _getBeatmap;
            }
        }
        public PreviewBeatmapLevelStub(string levelId)
        {
            levelID = levelId;
        }

        public PreviewBeatmapLevelStub(string levelId, string songName, string levelAuthorName)
            : this(levelId)
        {
            this.songName = songName;
            this.levelAuthorName = levelAuthorName;
        }

        public PreviewBeatmapLevelStub(string levelId, Beatmap beatmap)
        {
            levelID = levelId;
            Populate(beatmap);
        }

        private void Populate(Beatmap? beatmap)
        {
            if (beatmap != null)
            {
                songName = beatmap.Metadata.SongName;
                songSubName = beatmap.Metadata.SongSubName;
                songAuthorName = beatmap.Metadata.SongAuthorName;
                levelAuthorName = beatmap.Metadata.LevelAuthorName;
                beatsPerMinute = beatmap.Metadata.BPM;
                songDuration = beatmap.Metadata.Duration;
            }
            else
            {
                songName = "Not Found On BeatSaver!";
                levelID = "";
            }
        }

        public string levelID { get; private set; }

        public string? songName { get; private set; }

        public string? songSubName { get; private set; }

        public string? songAuthorName { get; private set; }

        public string? levelAuthorName { get; private set; }

        public float beatsPerMinute { get; private set; }

        public float songTimeOffset { get; private set; }

        public float shuffle { get; private set; }

        public float shufflePeriod { get; private set; }

        public float previewStartTime { get; private set; }

        public float previewDuration { get; private set; }

        public float songDuration { get; private set; }

        public EnvironmentInfoSO? environmentInfo { get; private set; }

        public EnvironmentInfoSO? allDirectionsEnvironmentInfo { get; private set; }

        public PreviewDifficultyBeatmapSet[]? previewDifficultyBeatmapSets { get; private set; }

        public async Task<Sprite?> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            Beatmap? bm = await GetBeatmap;
            if (bm != null)
            {
                var img = await bm.FetchCoverImage(cancellationToken);
                return Utilities.Utils.GetSprite(img);
            }
            else
            {
                return Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 2, 2), new Vector2(0, 0), 100.0f);
            }
        }

        public Task<AudioClip?> GetPreviewAudioClipAsync(CancellationToken cancellationToken)
        {
            var bm = SongCore.Loader.GetLevelById(levelID);
            if (bm != null)
            {
                return bm.GetPreviewAudioClipAsync(cancellationToken);
            }
            return Task.FromResult<AudioClip?>(null);
        }
    }
}
