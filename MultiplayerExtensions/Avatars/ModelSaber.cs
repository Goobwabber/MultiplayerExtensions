using CustomAvatar.Avatar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Avatars
{
    public static class ModelSaber
    {
        public static event Action<string> avatarDownloaded;
        public static Dictionary<string, LoadedAvatar> cachedAvatars = new Dictionary<string, LoadedAvatar>();

        public static Task<string> HashAvatar(LoadedAvatar avatar)
        {
            return Task.Run(() => {
                var hash = BitConverter.ToString(MD5.Create().ComputeHash(File.ReadAllBytes(Path.Combine(Path.GetFullPath("CustomAvatars"), avatar.fullPath)))).Replace("-", "");
                cachedAvatars[hash] = avatar;
                return hash;
            });
        }
    }
}
