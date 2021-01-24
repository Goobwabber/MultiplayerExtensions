using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiplayerExtensions.Utilities;
using System.Collections.Concurrent;
using MultiplayerExtensions.Beatmaps;
using BeatSaverSharp;
#nullable enable

namespace MultiplayerExtensions
{
    public static class Downloader
    {
        public static readonly string CustomLevelsFolder = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Beat Saber_Data", "CustomLevels");
        internal static ConcurrentDictionary<string, Task<IPreviewBeatmapLevel?>> CurrentDownloads 
            = new ConcurrentDictionary<string, Task<IPreviewBeatmapLevel?>>(StringComparer.OrdinalIgnoreCase);

        public static bool TryGetDownload(string levelId, out Task<IPreviewBeatmapLevel?> task) 
            => CurrentDownloads.TryGetValue(levelId, out task);

        private static async Task<IPreviewBeatmapLevel?> DownloadSong(string hash, IProgress<double>? progress, CancellationToken cancellationToken)
        {
            Beatmap bm = await Plugin.BeatSaver.Hash(hash);

            if (bm == null)
            {
                Plugin.Log?.Warn($"Could not find song '{hash}' on Beat Saver.");
                return null;
            }

            byte[] beatmapBytes = await bm.ZipBytes(false);
            string folderPath = Utils.GetSongDirectoryName(bm.Key, bm.Metadata.SongName, bm.Metadata.SongAuthorName);
            folderPath = Path.Combine(CustomLevelsFolder, folderPath);
            using (var ms = new MemoryStream(beatmapBytes))
            {
                var result = await ZipUtils.ExtractZip(ms, folderPath);
                if (folderPath != result.OutputDirectory)
                    folderPath = result.OutputDirectory ?? throw new Exception("Zip extract failed, no output directory.");
                if (result.Exception != null)
                    throw result.Exception;
            }
            Plugin.Log.Info($"Downloaded song to '{folderPath}'");

            using (var awaiter = new EventAwaiter<SongCore.Loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel>>(cancellationToken))
            {
                try
                {
                    SongCore.Loader.SongsLoadedEvent += awaiter.OnEvent;

                    SongCore.Collections.AddSong($"custom_level_{hash}", folderPath);
                    SongCore.Loader.Instance.RefreshSongs(false);
                    await awaiter.Task;
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
                Plugin.Log?.Warn($"Couldn't get downloaded beatmap '{bm.Metadata.SongName ?? hash}' from SongCore, this shouldn't happen.");
            return beatmap;
        }

        public static Task<IPreviewBeatmapLevel?> TryDownloadSong(string levelId, IProgress<double>? progress, CancellationToken cancellationToken)
        {
            Task<IPreviewBeatmapLevel?> task = CurrentDownloads.GetOrAdd(levelId, TryDownloadSongInternal(levelId, progress, cancellationToken));
            Plugin.Log?.Debug($"Active downloads: {CurrentDownloads.Count}");
            if (task.IsCompleted)
                CurrentDownloads.TryRemove(levelId, out _);
            return task;
        }

        private static async Task<IPreviewBeatmapLevel?> TryDownloadSongInternal(string levelId, IProgress<double>? progress, CancellationToken cancellationToken)
        {
            try
            {
                string? hash = Utils.LevelIdToHash(levelId);
                if(hash == null)
                {
                    Plugin.Log?.Error($"Cannot parse a hash from level id '{levelId}'.");
                    return null;
                }
                IPreviewBeatmapLevel? beatmap = await DownloadSong(hash, progress, cancellationToken);
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
