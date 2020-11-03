using MultiplayerExtensions.Avatars;
using System.Reflection;

namespace MultiplayerExtensions.Downloaders
{
    class AvatarDownloader
    {
        public static async void DownloadAvatar(string hash)
        {
            ModelSaber.FetchAvatarByHash(hash, delegate(ModelSaber.Avatar avatar)
            {
                avatar.DownloadAvatar(delegate (string avatarPath)
                {
                    ModelSaber.LoadAvatar(avatarPath);
                });
            });
        }
    }
}
