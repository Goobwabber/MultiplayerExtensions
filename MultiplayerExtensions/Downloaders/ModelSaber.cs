using CustomAvatar.Avatar;
using CustomAvatar.Player;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace MultiplayerExtensions.Downloaders
{
    public static class ModelSaber
    {
        public static event Action<string> avatarDownloaded;
        public static Dictionary<string, LoadedAvatar> cachedAvatars = new Dictionary<string, LoadedAvatar>();

        public static event Action hashesCalculated;

        public static bool isCalculatingHashes;
        public static int calculatedHashesCount;
        public static int totalAvatarsCount;

        public class Avatar
        {
            public string[] tags;
            public string type;
            public string name;
            public string author;
            public string image;
            public string hash;
            public string bsaber;
            public string download;
            public string install_link;
            public string date;

            public IEnumerator DownloadAvatar(Action<string> callback)
            {
                UnityWebRequest www = UnityWebRequest.Get(download);
                www.SetRequestHeader("User-Agent", UserAgent);
                www.timeout = 0;

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Plugin.Log?.Error($"Unable to download avatar! {www.error}");
                    yield break;
                }

                Plugin.Log.Debug("Received response from ModelSaber...");
                string docPath = "";
                string customAvatarPath = "";

                byte[] data = www.downloadHandler.data;

                try
                {
                    docPath = Application.dataPath;
                    docPath = docPath.Substring(0, docPath.Length - 5);
                    docPath = docPath.Substring(0, docPath.LastIndexOf("/"));
                    customAvatarPath = docPath + "/CustomAvatars/" + name + ".avatar";

                    Plugin.Log?.Debug($"Saving avatar to \"{customAvatarPath}\"...");

                    File.WriteAllBytes(customAvatarPath, data);
                    Plugin.Log?.Debug("Downloaded avatar!");

                    callback(name);
                }
                catch (Exception e)
                {
                    Plugin.Log?.Critical(e);
                    yield break;
                }
            }
        }

        public static IEnumerator FetchAvatarByHash(string hash, Action<Avatar> callback)
        {
            UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v1/avatar/get.php?filter=hash:" + hash);
            www.SetRequestHeader("User-Agent", UserAgent);

            www.timeout = 10;

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Plugin.Log?.Error($"Unable to fetch avatar! {www.error}");
                yield break;
            }

            Plugin.Log.Debug("Received response from ModelSaber...");
            Avatar avatar = JsonConvert.DeserializeObject<Dictionary<string, Avatar>>(www.downloadHandler.text).First().Value;
            callback(avatar);
        }

        [Inject]
        private static AvatarLoader _avatarLoader;

        public static string UserAgent { get; private set; }

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
                        LoadAvatar(avatarFile, false);
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

        public static void LoadAvatar(string avatarFile, bool downloaded = true)
        {
            _avatarLoader.FromFileCoroutine(avatarFile, async (LoadedAvatar avatar) =>
            {
                try
                {
                    string calculatedHash = await HashAvatar(avatar);
                    cachedAvatars.Add(calculatedHash, avatar);
                    Plugin.Log?.Debug($"Hashed avatar \"{avatar.descriptor.name}\"! Hash: {calculatedHash}");

                    if (downloaded)
                    {
                        HMMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            avatarDownloaded?.Invoke(calculatedHash);
                        });
                    }
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
    }
}
