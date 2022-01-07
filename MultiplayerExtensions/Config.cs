using IPA.Config.Stores.Attributes;

namespace MultiplayerExtensions
{
    public class Config
    {
        public virtual bool SingleplayerHUD { get; set; } = false;
        public virtual bool Hologram { get; set; } = true;
        public virtual bool LagReducer { get; set; } = false;
        public virtual bool MissLighting { get; set; } = true;
        public virtual string Color { get; set; } = "#08C0FF";
    }
}
