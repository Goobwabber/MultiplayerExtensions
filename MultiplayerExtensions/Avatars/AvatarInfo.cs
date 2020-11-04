using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiplayerExtensions.Avatars
{

    public class AvatarInfo
    {
        public string[]? tags;
        public string? type;
        public string? name;
        public string? author;
        public string? image;
        public string? hash;
        public string? bsaber;
        public string? download;
        public string? install_link;
        public string? date;

        public async Task<string?> DownloadAvatar(CancellationToken cancellationToken)
        {
            if (Uri.TryCreate(download, UriKind.Absolute, out Uri uri))
            {
                string? customAvatarPath = null;
                try
                {
                    HttpClient client = Utilities.Util.HttpClient;
                    HttpResponseMessage? response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string avatarDirectory = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "CustomAvatars");
                    Directory.CreateDirectory(avatarDirectory);
                    // TODO: May be better to download to temp directory first?
                    if (string.IsNullOrWhiteSpace(name))
                        name = "Unknown";
                    customAvatarPath = Path.Combine(avatarDirectory, $"{name}.avatar");
                    int index = 2;
                    while (File.Exists(customAvatarPath))
                    {
                        customAvatarPath = Path.Combine(avatarDirectory, $"{name}_{index}.avatar");
                    }
                    using (var fs = File.Create(customAvatarPath))
                    {
                        await response.Content.CopyToAsync(fs).ConfigureAwait(false);
                    }
                    return customAvatarPath;
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"Error downloading avatar from '{uri}': {ex.Message}");
                    Plugin.Log?.Debug(ex);
                    if (customAvatarPath != null && File.Exists(customAvatarPath))
                    {
                        try
                        {
                            File.Delete(customAvatarPath);
                        }
                        catch (Exception e)
                        {
                            Plugin.Log?.Error($"Error trying to delete incomplete download at '{customAvatarPath}': {e.Message}");
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerator DownloadAvatar(Action<string> callback)
        {
            UnityWebRequest www = UnityWebRequest.Get(download);
            www.SetRequestHeader("User-Agent", Plugin.UserAgent);
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
                customAvatarPath = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "CustomAvatars", $"{name}.avatar");

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
}
