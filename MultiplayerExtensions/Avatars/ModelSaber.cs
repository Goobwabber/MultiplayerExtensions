using CustomAvatar.Avatar;
using CustomAvatar.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Avatars
{
    public static class ModelSaber
    {
        public static event Action<string> avatarDownloaded;
        public static Dictionary<string, LoadedAvatar> cachedAvatars = new Dictionary<string, LoadedAvatar>();

        public static event Action hashesCalculated;

        public static bool isCalculatingHashes;
        public static int calculatedHashesCount;
        public static int totalAvatarsCount;

        [Inject]
        private static AvatarLoader _avatarLoader;

        public static Task<string> HashAvatar(LoadedAvatar avatar)
        {
            return HashAvatarPath(avatar.fullPath);
        }

        public static Task<string> HashAvatarPath(string path)
        {
            return Task.Run(() => {
                Plugin.Log?.Debug($"Hashing avatar path {path}");
                string hash = BitConverter.ToString(MD5.Create().ComputeHash(File.ReadAllBytes(Path.Combine(Path.GetFullPath("CustomAvatars"), path)))).Replace("-", "");
                return hash;
            });
        }

        public static async void HashAllAvatars()
        {
            var avatarFiles = Directory.GetFiles(PlayerAvatarManager.kCustomAvatarsPath, "*.avatar");
            totalAvatarsCount = avatarFiles.Length;

            if (totalAvatarsCount != cachedAvatars.Count && !isCalculatingHashes)
            {
                isCalculatingHashes = true;
                Plugin.Log?.Debug($"Hashing avatars... {totalAvatarsCount} avatars found");
                try
                {
                    cachedAvatars.Clear();
                    calculatedHashesCount = 0;

                    foreach(string avatarFile in avatarFiles)
                    {
                        _avatarLoader.FromFileCoroutine(avatarFile, async (LoadedAvatar avatar) =>
                        {
                            try
                            {
                                string calculatedHash = await HashAvatar(avatar);
                                cachedAvatars.Add(calculatedHash, avatar);
                                Plugin.Log?.Debug($"Hashed avatar \"{avatar.descriptor.name}\"! Hash: {calculatedHash}");
                            }
                            catch (Exception ex)
                            {
                                Plugin.Log?.Error($"Unable to hash avatar \"{avatar.descriptor.name}\"! Exception: {ex}");
                            }
                            calculatedHashesCount++;
                        }, (Exception ex) =>
                        {
                            Plugin.Log?.Error($"Unable to load avatar! Exception: {ex}");
                            calculatedHashesCount++;
                        });
                    }

                    while (totalAvatarsCount != calculatedHashesCount)
                    {
                        await Task.Delay(11);
                    }

                    Plugin.Log?.Debug("All avatars hashed and loaded!");

                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        hashesCalculated?.Invoke();
                    });
                }
                catch (Exception e)
                {
                    Plugin.Log?.Error($"Unable to hash and load avatars! Exception: {e}");
                }
                isCalculatingHashes = false;
            }
        }
    }
}
