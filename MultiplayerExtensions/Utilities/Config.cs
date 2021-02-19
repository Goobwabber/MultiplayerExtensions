namespace MultiplayerExtensions
{
    public class PluginConfig
    {
        public bool VerticalHUD { get; set; } = false;
        public bool SingleplayerHUD { get; set; } = false;
        public bool Hologram { get; set; } = true;
        public bool CustomSongs { get; set; } = true;
        public bool FreeMod { get; set; } = false;
        public string Color { get; set; } = "#08C0FF";
        public int MaxPlayers { get; set; } = 10;
    }
}
