using System.Numerics;

namespace iogame.Util
{
    public static class Vector2Ext
    {
        public static Vector2 ClampMagnitude(this Vector2 a, float maxLength)
        {
            var mag = a.Length();
            if(mag < maxLength)
                return a;
                
            var normalized_x = a.X / mag;
            var normalized_y = a.Y / mag;

            return new Vector2(normalized_x * maxLength, normalized_y * maxLength);
        }
    }
}