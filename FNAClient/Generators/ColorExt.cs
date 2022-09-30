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
            g = byte.Parse(colorCode[2..2], NumberStyles.AllowHexSpecifier);
            b = byte.Parse(colorCode[4..2], NumberStyles.AllowHexSpecifier);
            a = alpha;
            return Color.FromNonPremultiplied(r, g, b, a);
        }
    }
}