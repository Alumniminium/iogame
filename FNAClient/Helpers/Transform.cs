using System;
using Microsoft.Xna.Framework;

namespace server.Helpers
{
    internal readonly struct Transform
    {
        public readonly float PositionX;
        public readonly float PositionY;
        public readonly float Sin;
        public readonly float Cos;

        public static readonly Transform Zero = new(0f, 0f, 0f);

        public Transform(Vector2 position, float angle)
        {
            PositionX = position.X;
            PositionY = position.Y;
            Sin = MathF.Sin(angle);
            Cos = MathF.Cos(angle);
        }

        public Transform(float x, float y, float angle)
        {
            PositionX = x;
            PositionY = y;
            Sin = MathF.Sin(angle);
            Cos = MathF.Cos(angle);
        }
    }
}