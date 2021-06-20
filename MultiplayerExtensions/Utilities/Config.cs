using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MultiplayerExtensions
{
    public class PluginConfig
    {
        public virtual bool VerticalHUD { get; set; } = false;
        public virtual bool SingleplayerHUD { get; set; } = false;
        public virtual bool Hologram { get; set; } = true;
        public virtual bool CustomSongs { get; set; } = true;
        public virtual bool FreeMod { get; set; } = false;
        public virtual bool LagReducer { get; set; } = false;
        public virtual bool MissLighting { get; set; } = true;
        public virtual bool HostPick { get; set; } = true;
        public virtual string Color { get; set; } = "#08C0FF";
        public virtual int MaxPlayers { get; set; } = 10;
        public virtual bool ReportMasterServer { get; set; } = true;
        public virtual bool Statistics { get; set; } = true;

        [UseConverter(typeof(ListConverter<string>))]
        [NonNullable]
        public virtual List<string> EmoteURLs { get; set; } = new List<string>
        {
                "https://cdn.discordapp.com/emojis/570247548259008536.png?v=1",
                "https://cdn.discordapp.com/emojis/833130134851682317.png?v=1"
        };

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual DebugConfig? DebugConfig { get; set; }
    }

    public class DebugConfig
    {
        public virtual bool FailDownloads { get; set; } = false;
        public virtual int MinDownloadTime { get; set; } = 0;
    }
}
