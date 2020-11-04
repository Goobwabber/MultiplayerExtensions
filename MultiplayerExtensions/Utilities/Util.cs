using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable

namespace MultiplayerExtensions.Utilities
{
    public static class Util
    {
        private static HttpClient? _httpClient;

        public static HttpClient HttpClient
        {
            get 
            { 
                if(_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.UserAgent
                        .ParseAdd(Plugin.UserAgent);
                }
                return _httpClient; 
            }
        }



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
    }
}
