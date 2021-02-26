using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MultiplayerExtensions.Utilities;
using System.Collections.Concurrent;
using BeatSaverSharp;
using System.Diagnostics;
#nullable enable

namespace MultiplayerExtensions
{
    public delegate void DownloadProgressChangedHandler(string songHash, double progress);
    public static class Downloader
    {
        private const string TimeSpanFormat = @"s\.fff\s";
        public static readonly string CustomLevelsFolder = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Beat Saber_Data", "CustomLevels");
        internal static ConcurrentDictionary<string, Task<IPreviewBeatmapLevel?>> CurrentDownloads
            = new ConcurrentDictionary<string, Task<IPreviewBeatmapLevel?>>(StringComparer.OrdinalIgnoreCase);

        public static bool TryGetDownload(string levelId, out Task<IPreviewBeatmapLevel?> task)
            => CurrentDownloads.TryGetValue(levelId, out task);

        public static event DownloadProgressChangedHandler? DownloadProgressChanged;

        private static async Task<IPreviewBeatmapLevel?> DownloadSong(string hash, CancellationToken cancellationToken)
        {
            Beatmap? bm = await Plugin.BeatSaver.Hash(hash);

            if (bm == null)
            {
                Plugin.Log?.Warn($"Could not find song '{hash}' on Beat Saver.");
                return null;
            }
            Plugin.Log.Info($"Attempting to download song '({bm.Key}) {bm.Name ?? hash}'");
#if DEBUG
            if((Plugin.Config.DebugConfig?.FailDownloads ?? false))
            {
                await Task.Delay(2000);
                Plugin.Log.Info("Simulating a failed download by returning null.");
                return null;
            }
#endif
            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] beatmapBytes = await bm.ZipBytes(false, new StandardRequestOptions()
            {
                Progress = new Progress<double>(d =>
                {
#if DEBUG
                    Plugin.Log.Debug($"Downloading '{hash}': {d}");
#endif
                    DownloadProgressChanged?.Invoke(hash, d);
                })
            });
#if DEBUG
            TimeSpan delay = TimeSpan.FromSeconds(Plugin.Config.DebugConfig?.MinDownloadTime ?? 0) - TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
            if (delay > TimeSpan.Zero)
            {
                Plugin.Log.Debug($"Waiting an additional {delay.ToString(TimeSpanFormat)} to 'finish' download.");
                await Task.Delay(delay);
                Plugin.Log.Debug($"Delay finished.");
            }
#endif
            string folderPath = Utils.GetSongDirectoryName(bm.Key, bm.Metadata.SongName, bm.Metadata.LevelAuthorName);
            folderPath = Path.Combine(CustomLevelsFolder, folderPath);
            using (var ms = new MemoryStream(beatmapBytes))
            {
                var result = await ZipUtils.ExtractZip(ms, folderPath);
                if (folderPath != result.OutputDirectory)
                    folderPath = result.OutputDirectory ?? throw new Exception("Zip extract failed, no output directory.");
                if (result.Exception != null)
                    throw result.Exception;
            }
            sw.Stop();
            Plugin.Log.Info($"Downloaded song to '{folderPath}' after {sw.Elapsed.ToString(TimeSpanFormat)}");

            using (var awaiter = new EventAwaiter<SongCore.Loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel>>(cancellationToken))
            {
                try
                {
                    SongCore.Loader.SongsLoadedEvent += awaiter.OnEvent;

                    SongCore.Collections.AddSong($"custom_level_{hash}", folderPath);
                    SongCore.Loader.Instance.RefreshSongs(false);
                    await awaiter.Task;
                    Plugin.DebugLog("SongCore has finished refreshing");
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception e)
                {
                    Plugin.Log?.Error($"Error waiting for songs to load: {e.Message}");
                    Plugin.Log?.Debug(e);
                    throw;
                }
                finally
                {
                    SongCore.Loader.SongsLoadedEvent -= awaiter.OnEvent;
                }
            }

            CustomPreviewBeatmapLevel? beatmap = SongCore.Loader.GetLevelByHash(hash);
            if (beatmap == null)
                Plugin.Log?.Warn($"Couldn't get downloaded beatmap '{bm?.Metadata?.SongName ?? hash}' from SongCore, this shouldn't happen.");
            return beatmap;
        }

        public static Task<IPreviewBeatmapLevel?> TryDownloadSong(string levelId, CancellationToken cancellationToken)
        {
            Task<IPreviewBeatmapLevel?> task = CurrentDownloads.GetOrAdd(levelId, TryDownloadSongInternal(levelId, cancellationToken));
            Plugin.Log?.Debug($"Active downloads: {CurrentDownloads.Count}");
            if (task.IsCompleted)
                CurrentDownloads.TryRemove(levelId, out _);
            return task;
        }

        private static async Task<IPreviewBeatmapLevel?> TryDownloadSongInternal(string levelId, CancellationToken cancellationToken)
        {
            Plugin.DebugLog($"TryDownloadSongInternal: {levelId}");
            try
            {
                string? hash = Utils.LevelIdToHash(levelId);
                if (hash == null)
                {
                    Plugin.Log?.Error($"Cannot parse a hash from level id '{levelId}'.");
                    return null;
                }
                IPreviewBeatmapLevel? beatmap = await DownloadSong(hash, cancellationToken);
                if (beatmap is CustomPreviewBeatmapLevel customLevel)
                {
                    Plugin.Log?.Debug($"Download was successful.");
                    return beatmap;
                }
                else
                    Plugin.Log?.Error($"beatmap:{beatmap?.GetType().Name} is not a CustomPreviewBeatmapLevel");
            }
            catch (OperationCanceledException)
            {
                Plugin.Log?.Debug($"Download was canceled.");
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"Error downloading beatmap '{levelId}': {ex.Message}");
                Plugin.Log?.Debug(ex);
            }
            finally
            {
                CurrentDownloads.TryRemove(levelId, out _);
            }
            Plugin.Log?.Debug($"Download was unsuccessful.");
            return null;
        }


    }
}
