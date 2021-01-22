using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions
{
    public class PluginConfig
    {
        public bool VerticalHUD { get; set; } = false;
        public bool SingleplayerHUD { get; set; } = false;
        public bool Hologram { get; set; } = true;
        public bool CustomSongs { get; set; } = true;
        public bool EnforceMods { get; set; } = true;
        public string Color { get; set; } = "#08C0FF";
    }
}
