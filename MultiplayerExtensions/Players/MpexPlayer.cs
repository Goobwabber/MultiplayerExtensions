using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerExtensions.Players
{
    public class MpexPlayer : INetSerializable
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
                color = new Color(0.031f, 0.752f, 1f); // Default color from game
            Color = color;
        }
    }
}
