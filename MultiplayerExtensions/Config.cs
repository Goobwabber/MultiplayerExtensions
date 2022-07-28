using IPA.Config.Stores.Attributes;
using MultiplayerExtensions.Utilities;
using UnityEngine;

namespace MultiplayerExtensions
{
    public class Config
    {
        public static readonly Color DefaultPlayerColor = new Color(0.031f, 0.752f, 1f);

        public virtual bool SoloEnvironment { get; set; } = false;
        public virtual bool SideBySide { get; set; } = false;
        public virtual float SideBySideDistance { get; set; } = 4f;
        public virtual bool DisableAvatarConstraints { get; set; } = false;
        public virtual bool DisableMultiplayerPlatforms { get; set; } = false;
        public virtual bool DisableMultiplayerLights { get; set; } = false;
        public virtual bool DisableMultiplayerObjects { get; set; } = false;
        public virtual bool DisableMultiplayerColors { get; set; } = false;
        public virtual bool MissLighting { get; set; } = false;
        public virtual bool PersonalMissLightingOnly { get; set; } = false;
        [UseConverter(typeof(ColorConverter))]
        public virtual Color PlayerColor { get; set; } = DefaultPlayerColor;
        [UseConverter(typeof(ColorConverter))]
        public virtual Color MissColor { get; set; } = new Color(1, 0, 0);
    }
}
