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

        public static async Task<bool> PlayMap(string? userId, int platform, string hostname, string hash, string difficulty, string characteristic, int timePlayed, bool pass, int cutNotes, int missedNotes, int score, double accuracy)
        {
            if (userId != null)
                return await PerformWebRequest("POST", $"/playmap?userId={userId}&platform={platform}&hostname={hostname}&hash={hash}&difficulty={difficulty}&characteristic={characteristic}&timePlayed={timePlayed}&pass={pass}&cutNotes={cutNotes}&missedNotes={missedNotes}&score={score}&accuracy={accuracy}") != null;
            return true;
        }

        public static async Task<bool> UseMaster(string? userId, int platform, string hostname, bool host = false)
        {
            if (userId != null)
                return await PerformWebRequest("POST", $"/usemaster?userId={userId}&hostname={hostname}&platform={platform}&host={host}&disableNewMaster={!Plugin.Config.ReportMasterServer}") != null;
            return true;
        }
    }
}
