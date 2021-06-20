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
            "https://cdn.discordapp.com/emojis/445580723362201600.png?v=1",
            "https://cdn.discordapp.com/emojis/445580723362201600.png?v=1",
            "https://cdn.discordapp.com/emojis/585552569921699851.png?v=1",
            "https://cdn.discordapp.com/emojis/586303539731628043.png?v=1",
            "https://cdn.discordapp.com/emojis/585554863442755585.png?v=1",
            "https://cdn.discordapp.com/emojis/448732050636275722.png?v=1",
            "https://cdn.discordapp.com/emojis/638440635103182869.png?v=1",
            "https://cdn.discordapp.com/emojis/854887263318048828.png?v=1",
            "https://media.discordapp.net/attachments/850472473417220136/856086706775654440/unknown.png",
            "https://cdn.discordapp.com/emojis/570247548259008536.png?v=1",
            "https://cdn.discordapp.com/emojis/833130134851682317.png?v=1",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086710475030579/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086712522113054/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086714804338708/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086716295938050/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086719325536276/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086723313926144/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086724130897940/unknown.png",
            "https://cdn.discordapp.com/attachments/850472473417220136/856086726438158336/unknown.png",
            "https://cdn.discordapp.com/emojis/566921711040200724.png?v=1",
            "https://cdn.discordapp.com/emojis/527903893976514560.png?v=1",
            "https://cdn.discordapp.com/emojis/595235335198343184.png?v=1",
            "https://cdn.discordapp.com/emojis/630451225405423617.png?v=1",
            "https://media.discordapp.net/attachments/850472473417220136/856089265229660200/640px-Gay_Pride_Flag.png",
            "https://media.discordapp.net/attachments/850472473417220136/856089347282829322/1200px-Transgender_Pride_flag.png"
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
