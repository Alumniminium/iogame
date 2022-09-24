using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RG351MP.Generators.BmFont.Models;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace RG351MP.Generators.BmFont
{
    public class BmpFont
    {
        private readonly Texture2D fontTexture;
        private readonly Dictionary<char, BmpFontChar> _characterMap;

        public BmpFont(string fontName, ContentManager contentManager)
        {
            _characterMap = new Dictionary<char, BmpFontChar>();
            var fontFilePath = Path.Combine(contentManager.RootDirectory, fontName);
            fontTexture = contentManager.Load<Texture2D>(Path.GetFileName(fontName).Replace(".fnt", "_0"));
            Load(fontFilePath);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Load(string filename)
        {
            XmlSerializer deserializer = new(typeof(FontFile));
            TextReader textReader = new StreamReader(filename);
            FontFile file = (FontFile)deserializer.Deserialize(textReader);
            textReader.Close();
            foreach (var fontCharacter in file.Chars)
            {
                char c = (char)fontCharacter.ID;
                _characterMap.Add(c, new BmpFontChar(fontCharacter));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawText(SpriteBatch spriteBatch, float x, float y, string text, Color color, float scale = 0.5f)
        {
            int dx = (int)x;
            int dy = (int)y;
            foreach (char c in text)
            {
                if (_characterMap.TryGetValue(c, out var fc))
                {
                    var dst = new Rectangle(dx + (int)(fc.FontChar.XOffset * scale), dy + (int)(fc.FontChar.YOffset * scale), (int)(fc.SrcRect.Width * scale), (int)(fc.SrcRect.Height * scale));
                    spriteBatch.Draw(fontTexture, dst, fc.SrcRect, color);
                    dx += (int)(fc.FontChar.XAdvance * scale);
                }
            }
        }
    }
}