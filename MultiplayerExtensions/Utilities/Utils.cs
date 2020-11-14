using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable

namespace MultiplayerExtensions.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// 'custom_level_'
        /// </summary>
        public static readonly string CustomLevelIdPrefix = "custom_level_";
        /// <summary>
        /// Logger for debugging sprite loads.
        /// </summary>
        public static Action<string?, Exception?>? Logger;
        /// <summary>
        /// Creates a <see cref="Sprite"/> from an image <see cref="Stream"/>.
        /// </summary>
        /// <param name="imageStream"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <param name="returnDefaultOnFail"></param>
        /// <returns></returns>
        public static Sprite? GetSprite(byte[] data, float pixelsPerUnit = 100.0f, bool returnDefaultOnFail = true)
        {
            Sprite? ReturnDefault(bool useDefault)
            {
                return null;
            }
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                if (data == null || data.Length == 0)
                {
                    //Logger?.Invoke($"data seems to be null or empty.", null);
                    return ReturnDefault(returnDefaultOnFail);
                }
                texture.LoadImage(data);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), pixelsPerUnit);
            }
            catch (Exception ex)
            {
                Logger?.Invoke($"Caught unhandled exception", ex);
                return ReturnDefault(returnDefaultOnFail);
            }
        }

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
    }
}
