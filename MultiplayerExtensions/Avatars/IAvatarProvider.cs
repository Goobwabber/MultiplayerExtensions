using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Avatars
{
    public interface IAvatarProvider
    {
        event EventHandler<AvatarDownloadedEventArgs>? avatarDownloaded;
        event EventHandler? hashesCalculated;
        Type AvatarType { get; }
        bool isCalculatingHashes { get; }
        int cachedAvatarsCount { get; }
        string AvatarDirectory { get; }
        Task<string> HashAvatar(string path);
        Task HashAllAvatars(string directory);
        bool CacheAvatar(string avatarPath);
        Task<AvatarInfo?> FetchAvatarInfoByHash(string hash, CancellationToken cancellationToken);
    }

    public interface IAvatarProvider<T> : IAvatarProvider where T : class
    {
        bool TryGetCachedAvatar(string hash, out T avatar);
        Task<T?> LoadAvatar(string avatarPath);
        Task<string> HashAvatar(T avatar);
        Task<T?> FetchAvatarByHash(string hash, CancellationToken cancellationToken);
    }


    public class AvatarDownloadedEventArgs : EventArgs
    {
        public readonly string Hash;

        public AvatarDownloadedEventArgs(string hash)
        {
            Hash = hash;
        }
    }
}
