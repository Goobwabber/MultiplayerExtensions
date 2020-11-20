using BeatSaverSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Beatmaps
{
    static class PreviewBeatmapManager 
    {
        private static Dictionary<string, bool> localDownloadable = new Dictionary<string, bool>();
        private static List<PreviewBeatmapStub> cachedPreviews = new List<PreviewBeatmapStub>();

        public static PreviewBeatmapStub CreatePreview(PreviewBeatmapPacket packet)
        {
            if (localDownloadable.ContainsKey(packet.levelId))
                return new PreviewBeatmapStub(packet.levelId, localDownloadable[packet.levelId]);

            if (CacheContains(packet.levelId))
                return GetFromCache(packet.levelId);

            PreviewBeatmapStub preview = new PreviewBeatmapStub(packet);
            CachePreview(preview);
            return preview;
        }

        public static async Task<PreviewBeatmapStub> CreatePreview(string levelId)
        {
            if (localDownloadable.ContainsKey(levelId))
                return new PreviewBeatmapStub(levelId, localDownloadable[levelId]);

            if (CacheContains(levelId))
                return GetFromCache(levelId);

            if (SongCore.Loader.GetLevelById(levelId) != null)
            {
                PreviewBeatmapStub preview = new PreviewBeatmapStub(levelId);
                bool downloadable = await preview.isDownloadable;
                localDownloadable[levelId] = downloadable;
                return preview;
            }
            else
            {
                string? levelHash = Utilities.Utils.LevelIdToHash(levelId);
                Beatmap bm = await BeatSaver.Client.Hash(levelHash, CancellationToken.None);
                PreviewBeatmapStub preview = new PreviewBeatmapStub(levelId, bm);
                CachePreview(preview);
                return preview;
            }
        }

        private static void CachePreview(PreviewBeatmapStub preview)
        {
            if (cachedPreviews.Count >= 16)
                cachedPreviews.RemoveAt(16);
            cachedPreviews.Insert(0, preview);
        }

        private static bool CacheContains(string levelId)
            => cachedPreviews.Any(x => x.levelID == levelId);

        private static PreviewBeatmapStub GetFromCache(string levelId) 
            => cachedPreviews.Where(x => x.levelID == levelId).First();
    }
}
