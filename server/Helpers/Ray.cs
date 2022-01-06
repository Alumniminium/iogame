using System.Drawing;
using System.Numerics;

namespace server.Helpers
{
    public class Ray
    {
        Vector2 StartPosition;
        Vector2 Direction;
        public Ray(Vector2 from, float angleDeg)
        {
            StartPosition = from;
            Direction = angleDeg.AsVectorFromDegrees();
        }

        public void LookAt(float x, float y) => LookAt(new Vector2(x, y));
        public void LookAt(Vector2 point)
        {
            Direction.X = point.X - StartPosition.X;
            Direction.Y = point.Y - StartPosition.Y;
            Direction = Vector2.Normalize(Direction);
        }

        public Vector2 Cast(Rectangle rect)
        {
            var x1 = rect.Left;
            var y1 = rect.Top;
            var x2 = rect.Right;
            var y2 = rect.Bottom;

            var x3 = StartPosition.X;
            var y3 = StartPosition.Y;
            var x4 = StartPosition.X + Direction.X;
            var y4 = StartPosition.Y + Direction.Y;

            var den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (den == 0)
                return Vector2.Zero;

            var t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            var u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
            if (t <= 0 || t >= 1 || u <= 0)
                return Vector2.Zero;
                
            var pt = new Vector2
            {
                X = x1 + t * (x2 - x1),
                Y = y1 + t * (y2 - y1)
            };
            return pt;
        }
    }

}