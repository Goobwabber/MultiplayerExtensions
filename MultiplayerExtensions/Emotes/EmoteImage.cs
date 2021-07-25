using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Emotes
{
    public class EmoteImage
    {
        public string URL { get; private set; }
        public bool Blacklist { get; private set; }
        private Sprite _sprite;
        private bool SpriteLoadQueued;
        private byte[] imageData = null;

        public bool SpriteWasLoaded { get; private set; }
        private SemaphoreSlim spriteLoadSemaphore;
        public event EventHandler SpriteLoaded;

        private static readonly object _loaderLock = new object();
        private static bool CoroutineRunning = false;
        private static readonly Queue<Action> SpriteQueue = new Queue<Action>();

        public EmoteImage(string url)
        {
            URL = url;
            Blacklist = false;
            SpriteWasLoaded = false;
            spriteLoadSemaphore = new SemaphoreSlim(0, 1);
            SpriteLoadQueued = false;
        }

        public Sprite Sprite
        {
            get
            {
                if (_sprite == null)
                {
                    if (!SpriteLoadQueued)
                    {
                        SpriteLoadQueued = true;
                        QueueLoadSprite(this);
                    }
                    return BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
                }
                return _sprite;
            }
        }

        public Stream GetCoverStream() => new MemoryStream(imageData);

        public static YieldInstruction LoadWait = new WaitForEndOfFrame();

        internal async Task DownloadImage()
        {
            Uri uri = new Uri(URL);
            using (var webClient = new WebClient())
            {
                imageData = await webClient.DownloadDataTaskAsync(uri);
            }
        }

        public async Task WaitSpriteLoadAsync()
        {
            _ = Sprite;
            await spriteLoadSemaphore.WaitAsync();
            spriteLoadSemaphore.Release();
        }

        private static void QueueLoadSprite(EmoteImage emoteImage)
        {
            SpriteQueue.Enqueue(() =>
            {
                try
                {
                    using (Stream imageStream = emoteImage.GetCoverStream())
                    {
                        byte[] imageBytes = new byte[imageStream.Length];
                        imageStream.Read(imageBytes, 0, (int)imageStream.Length);
                        emoteImage._sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                        if (emoteImage._sprite != null)
                        {
                            emoteImage.SpriteWasLoaded = true;
                            emoteImage._sprite.texture.wrapMode = TextureWrapMode.Clamp;
                        }
                        else
                        {
                            Plugin.Log.Critical("Could not load " + emoteImage.URL);
                            emoteImage.SpriteWasLoaded = false;
                            emoteImage.Blacklist = true;
                        }
                        emoteImage.SpriteLoaded?.Invoke(emoteImage, null);
                        emoteImage.spriteLoadSemaphore.Release();
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Critical("Could not load " + emoteImage.URL + "\nException message: " + e.Message);
                    emoteImage.SpriteWasLoaded = false;
                    emoteImage.Blacklist = true;
                    emoteImage.SpriteLoaded?.Invoke(emoteImage, null);
                    emoteImage.spriteLoadSemaphore.Release();
                }
            });

            if (!CoroutineRunning)
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }

        private static IEnumerator<YieldInstruction> SpriteLoadCoroutine()
        {
            lock (_loaderLock)
            {
                if (CoroutineRunning)
                    yield break;
                CoroutineRunning = true;
            }
            while (SpriteQueue.Count > 0)
            {
                yield return LoadWait;
                var loader = SpriteQueue.Dequeue();
                loader?.Invoke();
            }
            CoroutineRunning = false;
            if (SpriteQueue.Count > 0)
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }
    }
}
