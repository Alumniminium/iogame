using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace iogame.Simulation.Entities
{
    public static class Vector2Ext
    {
        public static Vector2 Unit(this Vector2 v)
        {
            if (v.Magnitude() == 0)
                return new Vector2(0, 0);
            return new Vector2(v.X / v.Magnitude(), v.Y / v.Magnitude());
        }
        public static float Magnitude(this Vector2 vector) => (float)Math.Sqrt(SquareMagnitude(vector));
        public static float SquareMagnitude(this Vector2 vector) => vector.X * vector.X + vector.Y * vector.Y;
        public static Vector2 ClampMagnitude(this Vector2 a, float maxLength)
        {
            var mag = a.Magnitude();
            if(mag < maxLength)
                return a;
                
            var normalized_x = a.X / mag;
            var normalized_y = a.Y / mag;

            return new Vector2(normalized_x * maxLength, normalized_y * maxLength);
        }
    }
}