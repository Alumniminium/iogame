using Microsoft.Xna.Framework;

namespace RG351MP.Generators.BmFont.Models
{
    public class BmpFontChar
    {
        public FontChar FontChar { get; }
        public Rectangle SrcRect { get; }

        public BmpFontChar(FontChar fontCharacter)
        {
            FontChar = fontCharacter;
            SrcRect = new Rectangle(FontChar.X, FontChar.Y, FontChar.Width, FontChar.Height);
        }
    }
}