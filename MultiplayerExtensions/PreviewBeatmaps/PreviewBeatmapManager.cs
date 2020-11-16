using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Beatmaps
{
    static class PreviewBeatmapManager 
    {
        private static Dictionary<string, PreviewBeatmapStub> CachedPreviews = new Dictionary<string, PreviewBeatmapStub>();

        public static async Task<PreviewBeatmapStub> GetPopulatedPreview(string levelId)
        {
            if (CachedPreviews.ContainsKey(levelId))
            {
                Plugin.Log?.Debug($"PreviewBeatmap({levelId}): Cached preview found, skipping beatsaver request.");
                return CachedPreviews[levelId];
            }
            return await FetchPopulatedPreview(levelId);
        }

        public static PreviewBeatmapStub GetPreview(PreviewBeatmapPacket packet)
            => GetPreview(packet.levelId, packet.songName, packet.songSubName, packet.songAuthorName, packet.levelAuthorName);

        public static PreviewBeatmapStub GetPreview(string levelId, string songName, string songSubName, string songAuthorName, string levelAuthorName)
        {
            if (CachedPreviews.ContainsKey(levelId))
            {
                Plugin.Log?.Debug($"PreviewBeatmap({levelId}): Cached preview found, skipping beatsaver request.");
                return CachedPreviews[levelId];
            }
            return FetchPreview(levelId, songName, songSubName, songAuthorName, levelAuthorName);
        }

        public static async Task<PreviewBeatmapStub> FetchPopulatedPreview(string levelId)
        {
            PreviewBeatmapStub beatmap = new PreviewBeatmapStub(levelId);
            if (!beatmap.isDownloaded)
            {
                await beatmap.FetchPopulated();
            }
            else
            {
                _ = beatmap.FetchPopulated();
            }

            CachedPreviews.Add(levelId, beatmap);
            return beatmap;
        }

        public static PreviewBeatmapStub FetchPreview(string levelId, string songName, string songSubName, string songAuthorName, string levelAuthorName)
        {
            PreviewBeatmapStub beatmap = new PreviewBeatmapStub(levelId, songName, songSubName, songAuthorName, levelAuthorName);
            _ = beatmap.FetchPopulated();
            CachedPreviews.Add(levelId, beatmap);
            return beatmap;
        }
    }
}
