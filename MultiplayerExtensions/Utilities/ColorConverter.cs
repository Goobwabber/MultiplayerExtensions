using IPA.Config.Data;
using IPA.Config.Stores;
using System;
using UnityEngine;

namespace MultiplayerExtensions.Utilities
{
    public class ColorConverter : ValueConverter<Color>
    {
        public override Color FromValue(Value? value, object parent)
        {
            if (value is not Text text) 
                throw new ArgumentException("Argument not Text", nameof(value));
            if (!ColorUtility.TryParseHtmlString(text.Value, out var color))
                throw new ArgumentException("Could not parse HtmlString", nameof(value));
            return color;
        }

        public override Value? ToValue(Color obj, object parent)
            => Value.Text($"#{ColorUtility.ToHtmlStringRGB(obj)}");
    }
}
