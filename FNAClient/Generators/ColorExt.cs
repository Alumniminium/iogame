using System.Globalization;
using Microsoft.Xna.Framework;

namespace RG351MP
{
    public static class ColorExt
    {
        public static Color ToColor(this string colorCode, byte alpha = 255)
        {
            if (colorCode.IndexOf("#") != -1)
                colorCode = colorCode.Replace("#", "");
            byte r, g, b, a;
            r = byte.Parse(colorCode[..2], NumberStyles.AllowHexSpecifier);
            g = byte.Parse(colorCode.Substring(2,2), NumberStyles.AllowHexSpecifier);
            b = byte.Parse(colorCode.Substring(4,2), NumberStyles.AllowHexSpecifier);
            a = alpha;
            return Color.FromNonPremultiplied(r, g, b, a);
        }
        public static Color ToColor(uint value)
        {
            return new Color((byte)((value >> 24) & 0xFF),
                       (byte)((value >> 16) & 0xFF),
                       (byte)((value >> 8) & 0xFF),
                       (byte)(value & 0xFF));
        }
    }
}