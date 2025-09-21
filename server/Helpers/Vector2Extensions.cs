using System.Numerics;

namespace server.Helpers;

// Helper extension for cross product in 2D
public static class Vector2Extensions
{
    public static float Cross(this Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }
}