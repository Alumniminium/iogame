using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RG351MP.Scenes;

namespace RG351MP
{
    public class SpringCamera
    {
        private Vector2 _position;
        private Vector2 _halfScreenSize;
        private Viewport _viewport;
        public float Scale;

        public SpringCamera(Viewport viewport)
        {
            Scale = 2;
            Transform = Matrix.Identity;
            _viewport = viewport;
            _halfScreenSize = new Vector2(Viewport.Width / 2, Viewport.Height / 2);
        }

        public Matrix Transform { get; private set; }

        public Viewport Viewport
        {
            get { return _viewport; }
            set
            {
                _viewport = value;
                _halfScreenSize = new Vector2(Viewport.Width / 2, Viewport.Height / 2);
            }
        }


        public void Update(float rotation, Vector2 desiredPosition)
        {
            _position = desiredPosition;
            
            // contrain camera to map size with scale
            _position.X = MathHelper.Clamp(_position.X, _halfScreenSize.X / Scale, GameScene.MapSize.X - _halfScreenSize.X / Scale);
            _position.Y = MathHelper.Clamp(_position.Y, _halfScreenSize.Y / Scale, GameScene.MapSize.Y - _halfScreenSize.Y / Scale);

            Transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                        Matrix.CreateRotationZ(rotation) *
                        Matrix.CreateScale(Scale) *
                        Matrix.CreateTranslation(_halfScreenSize.X, _halfScreenSize.Y, 0);
        }
        public Vector2 ScreenToWorld(Vector2 p)
        {
            return new Vector2((p.X / Viewport.X) + Viewport.Bounds.Left, (p.Y / Viewport.Y) + Viewport.Bounds.Top);
        }

        public Vector2 WorldToScreen(Vector2 p)
        {
            return Vector2.Transform(p,Transform);
        }
    }
}