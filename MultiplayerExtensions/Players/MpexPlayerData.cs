using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerExtensions.Players
{
    public class MpexPlayerData : INetSerializable
    {
        /// <summary>
        /// Player's color set in the plugin config.
        /// </summary>
        public Color Color { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put("#" + ColorUtility.ToHtmlStringRGB(Color));
        }

        public void Deserialize(NetDataReader reader)
        {
            Color color;
            if (!ColorUtility.TryParseHtmlString(reader.GetString(), out color))
                color = Config.DefaultPlayerColor;
            Color = color;
        }
    }
}
