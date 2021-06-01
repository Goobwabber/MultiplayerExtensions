using Newtonsoft.Json;

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
        public virtual bool HostPick { get; set; } = true;
        public virtual string Color { get; set; } = "#08C0FF";
        public virtual int MaxPlayers { get; set; } = 10;
        public virtual bool ReportMasterServer { get; set; } = true;
        public virtual bool Statistics { get; set; } = true;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual DebugConfig? DebugConfig { get; set; }
    }

    public class DebugConfig
    {
        public virtual bool FailDownloads { get; set; } = false;
        public virtual int MinDownloadTime { get; set; } = 0;
    }
}
