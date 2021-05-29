using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Utilities
{
    public static class Statistics
    {
        private const string BASE_URL = "https://mpex.goobwabber.com/api/v1";

        // From com.github.roydejong.BeatSaberServerBrowser
        private static async Task<HttpResponseMessage?> PerformWebRequest(string method, string endpoint, string json = null)
        {
            var targetUrl = BASE_URL + endpoint;
            Plugin.Log?.Debug($"{method} {targetUrl} {json}");

            try
            {
                HttpResponseMessage response;

                switch (method)
                {
                    case "GET":
                        response = await Plugin.HttpClient.GetAsync(targetUrl).ConfigureAwait(false);
                        break;
                    case "POST":
                        if (String.IsNullOrEmpty(json))
                        {
                            response = await Plugin.HttpClient.PostAsync(targetUrl, null).ConfigureAwait(false);
                        }
                        else
                        {
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            response = await Plugin.HttpClient.PostAsync(targetUrl, content).ConfigureAwait(false);
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid request method for the Master Server API: {method}");
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Expected HTTP 200 OK, got HTTP {response.StatusCode}");
                }

                Plugin.Log?.Debug($"✔ 200 OK: {method} {targetUrl}");
                return response;
            }
            catch (TaskCanceledException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"⚠ Request error: {method} {targetUrl} → {ex}");
                return null;
            }
        }

        public static async Task<bool> AddUser(string userId, int platform)
        {
            return await PerformWebRequest("POST", $"/adduser?userId={userId}&platform={platform}") != null;
        }

        public static async Task<bool> PlayMap(string hash, string difficulty, string characteristic, int timePlayed, int platform, string hostname)
        {
            return await PerformWebRequest("POST", $"/playmap?hash={hash}&difficulty={difficulty}&characteristic={characteristic}&timePlayed={timePlayed}&platform={platform}&hostname={hostname}") != null;
        }

        public static async Task<bool> UseMaster(string hostname, int platform, string? userId = null, bool host = false)
        {
            if (userId != null)
                return await PerformWebRequest("POST", $"/usemaster?hostname={hostname}&userId={userId}&platform={platform}&host={host}") != null;
            return await PerformWebRequest("POST", $"/usemaster?hostname={hostname}&platform={platform}&host={host}") != null;
        }
    }
}
