using SiraUtil.Logging;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Utilities
{
    public class SpriteManager : IInitializable
    {
        public Sprite IconOculus64 { get; private set; } = null!;
        public Sprite IconSteam64 { get; private set; } = null!;

        private readonly SiraLog _logger;

        internal SpriteManager(
            SiraLog logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            IconOculus64 = GetSpriteFromResources("MultiplayerExtensions.Assets.IconOculus64.png");
            IconSteam64 = GetSpriteFromResources("MultiplayerExtensions.Assets.IconSteam64.png");
        }

        private Sprite GetSpriteFromResources(string resourcePath, float pixelsPerUnit = 10.0f)
        {
            Sprite? sprite = GetSprite(GetResource(Assembly.GetCallingAssembly(), resourcePath), pixelsPerUnit);
            if (sprite == null)
                return null!;
            sprite.name = resourcePath;
            return sprite;
        }

        private byte[] GetResource(Assembly asm, string resourceName)
        {
            System.IO.Stream stream = asm.GetManifestResourceStream(resourceName);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }

        public Sprite? GetSprite(byte[]? data, float pixelsPerUnit = 100.0f, bool returnDefaultOnFail = true)
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
                _logger.Warn($"Caught unhandled exception {ex.Message}");
                return ReturnDefault(returnDefaultOnFail);
            }
        }
    }
}
