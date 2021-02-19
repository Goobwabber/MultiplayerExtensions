using MultiplayerExtensions.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.OverrideClasses
{
    class LevelLoaderStub : MultiplayerLevelLoader, IProgress<double>
    {
        public event Action<double> progressUpdated;

        public override void LoadLevel(BeatmapIdentifierNetSerializable beatmapId, GameplayModifiers gameplayModifiers, float initialStartTime)
        {
            string? levelId = beatmapId.levelID;
            string? hash = Utilities.Utils.LevelIdToHash(beatmapId.levelID);
            if (SongCore.Collections.songWithHashPresent(hash) || hash == null)
            {
                Plugin.Log?.Debug($"(SongLoader) Level with ID '{levelId}' already exists.");
                base.LoadLevel(beatmapId, gameplayModifiers, initialStartTime);
                return;
            }
            if (Downloader.TryGetDownload(levelId, out _))
            {
                Plugin.Log?.Debug($"(SongLoader) Download for '{levelId}' is already in progress.");
                return;
            }

            Plugin.Log?.Debug($"(SongLoader) Attempting to download level with ID '{levelId}'...");
            DownloadSong(levelId).ContinueWith(r =>
            {
                if (r.Result == true)
                    base.LoadLevel(beatmapId, gameplayModifiers, initialStartTime);
            });
        }

        private async Task<bool> DownloadSong(string levelId)
        {
            try
            {
                IPreviewBeatmapLevel? beatmap = await Downloader.TryDownloadSong(levelId, this, CancellationToken.None);
                if (beatmap != null)
                {
                    Plugin.Log?.Debug($"(SongLoader) Level with ID '{levelId}' was downloaded successfully.");
                    return true;
                }
                Plugin.Log?.Warn($"(SongLoader) TryDownloadSong was unsuccessful.");
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Error in TryDownloadSong continuation: {ex.Message}");
                Plugin.Log?.Debug(ex);
            }
            return false;
        }

        void IProgress<double>.Report(double value)
            => progressUpdated?.Invoke(value);
    }
}
