using MultiplayerExtensions.Extensions;
using System;
using System.IO;
using System.Linq;
#nullable enable

namespace MultiplayerExtensions.Utilities
{
    public static class Utils
    {
        public static string? LevelIdToHash(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
                return null;
            string[] ary = levelId.Split('_', ' ');
            string? hash = null;
            if (ary.Length > 2)
                hash = ary[2];
            if ((hash?.Length ?? 0) == 40)
                return hash;
            return null;
        }

        public static string GetSongDirectoryName(string? songKey, string songName, string levelAuthorName)
        {
            // BeatSaverDownloader's method of naming the directory.
            string basePath;
            string nameAuthor;
            if (string.IsNullOrEmpty(levelAuthorName))
                nameAuthor = songName;
            else
                nameAuthor = $"{songName} - {levelAuthorName}";
            songKey = songKey?.Trim();
            if (songKey != null && songKey.Length > 0)
                basePath = songKey + " (" + nameAuthor + ")";
            else
                basePath = nameAuthor;
            basePath = string.Concat(basePath.Trim().Split(InvalidPathChars));
            return basePath;
        }

        private static readonly char[] _baseInvalidPathChars = new char[]
            {
                '<', '>', ':', '/', '\\', '|', '?', '*', '"',
                '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
                '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
                '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
                '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f',
            };

        private static char[]? _invalidPathChars;
        public static char[] InvalidPathChars
        {
            get
            {
                if (_invalidPathChars == null)
                {
                    _invalidPathChars = _baseInvalidPathChars.Concat(Path.GetInvalidPathChars()).Distinct().ToArray();
                }
                return _invalidPathChars;
            }
        }

        public static void RaiseEventSafe(this EventHandler? e, object sender, string eventName)
        {
            if (e == null) return;
            EventHandler[] handlers = e.GetInvocationList().Select(d => (EventHandler)d).ToArray()
                ?? Array.Empty<EventHandler>();
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    handlers[i].Invoke(sender, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"Error in '{eventName}' handlers '{handlers[i]?.Method.Name}': {ex.Message}");
                    Plugin.Log?.Debug(ex);
                }
            }
        }

        public static void RaiseEventSafe<TArgs>(this EventHandler<TArgs>? e, object sender, TArgs args, string eventName)
        {
            if (e == null) return;
            EventHandler<TArgs>[] handlers = e.GetInvocationList().Select(d => (EventHandler<TArgs>)d).ToArray()
                ?? Array.Empty<EventHandler<TArgs>>();
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    handlers[i].Invoke(sender, args);
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"Error in '{eventName}' handlers '{handlers[i]?.Method.Name}': {ex.Message}");
                    Plugin.Log?.Debug(ex);
                }
            }
        }

        public static Platform ToPlatform(this UserInfo.Platform platform)
        {
            return platform switch
            {
                UserInfo.Platform.Test => Platform.Unknown,
                UserInfo.Platform.Steam => Platform.Steam,
                UserInfo.Platform.Oculus => Platform.OculusPC,
                UserInfo.Platform.PS4 => Platform.PS4,
                _ => Platform.Unknown
            };
        }
    }
}
