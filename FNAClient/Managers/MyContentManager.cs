using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using RG351MP.Generators;
using RG351MP.Generators.BmFont;

namespace RG351MP.Managers
{
    public static class MyContentManager
    {
        private static ContentManager Content;
        public static BmpFont Font;
        public static Texture2D Pixel;
        public static Texture2D Ship;
        public static Texture2D Space;
        public static Video Video;

        public static void Initialize(ContentManager content)
        {
            Content = content;
            Content.RootDirectory = "Content";
        }
        public static void Load()
        {
            Font = new BmpFont("font.fnt", Content);
            Pixel = TextureGen.Pixel(GameEntry.DevMngr.GraphicsDevice,"#FFFFFF");
            Ship = Content.Load<Texture2D>("ship.png");
            Space = Content.Load<Texture2D>("space.png");
            // Video = Content.Load<Video>("FNAVideo");
        }
    }
}
