using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System;

namespace iogame.Simulation.Entities
{
    public static class Vector2Ext
    {
        public static Vector2 unit(this Vector2 v)
        {
            if (v.Magnitude() == 0)
                return new Vector2(0, 0);
            return new Vector2(v.X / v.Magnitude(), v.Y / v.Magnitude());
        }
        public static float Magnitude(this Vector2 vector) => (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        public static float SquareMagnitude(this Vector2 vector) => vector.X * vector.X + vector.Y * vector.Y;
        public static Vector2 ClampMagnitude(this Vector2 vector2, float maxLength)
        {
            var sqrmag = vector2.SquareMagnitude();
            if (sqrmag > maxLength * maxLength)
            {
                var mag = (float)Math.Sqrt(sqrmag);
                var normalized_x = vector2.X / mag;
                var normalized_y = vector2.Y / mag;

                return new Vector2(normalized_x * maxLength, normalized_y * maxLength);
            }

            return vector2;
        }
    }
}