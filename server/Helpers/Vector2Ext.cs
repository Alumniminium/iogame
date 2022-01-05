using System.Numerics;
using server.Helpers;

namespace server.Helpers
{
    public static class Vector2Ext
    {
        public static Vector2 ClampMagnitude(this Vector2 a, float maxLength)
        {
            if (maxLength == 0)
                return Vector2.Zero;
            var mag = a.Length();
            if (mag < maxLength)
                return a;

            var normalizedX = a.X / mag;
            var normalizedY = a.Y / mag;

            return new Vector2(normalizedX * maxLength, normalizedY * maxLength);
        }
        public static Vector2 AsVectorFromRadians(this float radians) => FromRadians(radians);
        public static Vector2 AsVectorFromDegrees(this float degrees) => FromDegrees(degrees);
        public static Vector2 FromRadians(float radians)
        {
            var x = MathF.Cos(radians);
            var y = MathF.Sin(radians);
            return new Vector2(x, y);
        }
        public static Vector2 FromDegrees(float degrees)
        {
            var radians = degrees * (MathF.PI / 180);
            var x = MathF.Cos(radians);
            var y = MathF.Sin(radians);
            return new Vector2(x, y);
        }
        public static float ToRadians(this Vector2 v) => MathF.Atan2(v.Y, v.X);
    }
}