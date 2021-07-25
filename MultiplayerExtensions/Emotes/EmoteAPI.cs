using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Emotes
{
    public class EmoteAPI
    {
        [JsonProperty("global")]
        public List<string> GlobalEmotes { get; private set; }

        public static async Task<EmoteAPI> GetEmoteAPIResultAsync(string userID)
        {
            Uri uri = new Uri($"https://mpex.goobwabber.com/api/v3/emotes?userId={userID}");
            try
            {
                using (var webClient = new WebClient())
                {
                    byte[] response = await webClient.DownloadDataTaskAsync(uri);
                    return JsonConvert.DeserializeObject<EmoteAPI>(Encoding.UTF8.GetString(response));
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"An error occurred while trying to fetch emotes\nException: {e}");
            }
            return null;
        }
    }
}
