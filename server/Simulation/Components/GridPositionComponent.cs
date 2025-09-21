using System;
using System.Numerics;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct GridPositionComponent
{
    public Vector2Int GridPos;
    public NTT Assembly;          // Ship this block belongs to
    public BlockRotation Rotation;  // 0, 90, 180, 270 degrees
}

public enum BlockRotation
{
    None = 0,      // Facing right (+X)
    Rotate90 = 1,  // Facing down (+Y in Box2D)
    Rotate180 = 2, // Facing left (-X)
    Rotate270 = 3  // Facing up (-Y in Box2D)
}

// Extension methods for rotation
public static class BlockRotationExtensions
{
    public static float ToRadians(this BlockRotation rotation)
        => rotation switch
        {
            BlockRotation.None => 0f,
            BlockRotation.Rotate90 => MathF.PI / 2f,
            BlockRotation.Rotate180 => MathF.PI,
            BlockRotation.Rotate270 => 3f * MathF.PI / 2f,
            _ => 0f
        };

    public static Vector2 GetDirection(this BlockRotation rotation)
        => rotation switch
        {
            BlockRotation.None => Vector2.UnitX,        // Right
            BlockRotation.Rotate90 => Vector2.UnitY,    // Down (Box2D coords)
            BlockRotation.Rotate180 => -Vector2.UnitX,  // Left
            BlockRotation.Rotate270 => -Vector2.UnitY,  // Up
            _ => Vector2.UnitX
        };

    public static BlockRotation RotateClockwise(this BlockRotation rotation)
        => (BlockRotation)(((int)rotation + 1) % 4);

    public static BlockRotation RotateCounterClockwise(this BlockRotation rotation)
        => (BlockRotation)(((int)rotation + 3) % 4);
}

// Struct for grid positions (since Vector2Int might not exist)
public struct Vector2Int
{
    public int X, Y;

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
    public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);

    public override bool Equals(object obj) => obj is Vector2Int other && this == other;
    public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 16);
    public override string ToString() => $"({X}, {Y})";
}