
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RG351MP
{
    public class SpringCamera
    {
        private Vector2 _position;
        private Vector2 _velocity;
        private Vector2 _halfScreenSize;
        private Viewport _viewport;
        public float Scale;

        public SpringCamera(Viewport viewport)
        {
            Scale = 10;
            Transform = Matrix.Identity;
            _viewport = viewport;
            _halfScreenSize = new Vector2(Viewport.Width / 2, Viewport.Height / 2);

            /* Values you can change to modify the camera reaction */
            Damping = 9f;
            SpringStiffness = 10f;
            Mass = 10f;
        }

        public Matrix Transform { get; private set; }

        public float Mass { get; private set; }

        public float SpringStiffness { get; set; }

        public float Damping { get; set; }

        public Viewport Viewport
        {
            get { return _viewport; }
        }


        public void Update(float elapsedSeconds, float rotation, Vector2 desiredPosition)
        {
            _position = desiredPosition;
            var x = _position - desiredPosition;
            var force = -SpringStiffness * x - Damping * _velocity;

            var acceleration = force / Mass;
            _velocity += acceleration * elapsedSeconds;
            _position += _velocity * elapsedSeconds;

            Transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                        Matrix.CreateRotationZ(rotation) *
                        Matrix.CreateScale(Scale) *
                        Matrix.CreateTranslation(_halfScreenSize.X, _halfScreenSize.Y, 0);
        }

        internal void Update(float v1, float v2, object position)
        {
            throw new NotImplementedException();
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