using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RG351MP.Generators
{
    public static class TextureGen
    {
        public static Texture2D Blank(GraphicsDevice device, int w, int h, Color color)
        {
            var _blankTexture = new Texture2D(device, w, h);
            var pixels = new Color[w * h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    pixels[y * w + x] = color;
            _blankTexture.SetData(pixels);
            var pixels2 = new Color[w * h];
            _blankTexture.GetData(pixels2);
            return _blankTexture;
        }
        public static Texture2D Pixel(GraphicsDevice device, string color) => Blank(device, 1, 1, color.ToColor());
    }
}