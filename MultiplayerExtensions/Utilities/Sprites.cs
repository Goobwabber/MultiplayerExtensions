using System;
using System.Reflection;
using UnityEngine;

namespace MultiplayerExtensions.Utilities
{
    class Sprites
    {
        /// <summary>
        /// Creates a <see cref="Sprite"/> from an image <see cref="Stream"/>.
        /// </summary>
        /// <param name="imageStream"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <param name="returnDefaultOnFail"></param>
        /// <returns></returns>
        public static Sprite? GetSprite(byte[]? data, float pixelsPerUnit = 100.0f, bool returnDefaultOnFail = true)
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
                Plugin.Log?.Warn($"Caught unhandled exception {ex.Message}");
                return ReturnDefault(returnDefaultOnFail);
            }
        }

        /// <summary>
        /// Gets the raw image data from a sprite in <see cref="byte[]"/> format.
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static byte[] GetRaw(Sprite sprite) => sprite.texture.GetRawTextureData();

        #region Resource Sprites
        public static Sprite IconOculus64 { get; private set; } = null!;
        public static Sprite IconSteam64 { get; private set; } = null!;
        
        public static void PreloadSprites()
        {
            IconOculus64 = GetSpriteFromResources("MultiplayerExtensions.Assets.IconOculus64.png");
            IconSteam64 = GetSpriteFromResources("MultiplayerExtensions.Assets.IconSteam64.png");
        }
        
        private static Sprite GetSpriteFromResources(string resourcePath, float pixelsPerUnit = 10.0f)
        {
            Sprite? sprite = GetSprite(GetResource(Assembly.GetCallingAssembly(), resourcePath), pixelsPerUnit);
            if (sprite == null)
                return null!;
            sprite.name = resourcePath;
            return sprite;
        }
        
        private static byte[] GetResource(Assembly asm, string resourceName)
        {
            System.IO.Stream stream = asm.GetManifestResourceStream(resourceName);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }
        #endregion
    }
}
