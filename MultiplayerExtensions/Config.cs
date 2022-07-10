using IPA.Config.Stores.Attributes;
using MultiplayerExtensions.Utilities;
using UnityEngine;

namespace MultiplayerExtensions
{
    public class Config
    {
        public static readonly Color DefaultPlayerColor = new Color(0.031f, 0.752f, 1f);

        public virtual bool SoloEnvironment { get; set; } = false;
        public virtual bool DisableAvatarConstraints { get; set; } = false;
        public virtual bool DisableMultiplayerPlatforms { get; set; } = false;
        public virtual bool LagReducer { get; set; } = false;
        public virtual bool MissLighting { get; set; } = true;
        [UseConverter(typeof(ColorConverter))]
        public virtual Color PlayerColor { get; set; } = DefaultPlayerColor;
        [UseConverter(typeof(ColorConverter))]
        public virtual Color MissColor { get; set; } = new Color(1, 0, 0);
    }
}
