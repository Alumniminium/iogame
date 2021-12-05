using System.Numerics;

namespace server.Helpers
{
    public static class Vector2Ext
    {
        public static Vector2 ClampMagnitude(this Vector2 a, float maxLength)
        {
            var mag = a.Length();
            if(mag < maxLength)
                return a;
                
            var normalizedX = a.X / mag;
            var normalizedY = a.Y / mag;

            return new Vector2(normalizedX * maxLength, normalizedY * maxLength);
        }
    }
}