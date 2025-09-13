using System.Numerics;

namespace server.Simulation.SpaceParition;

public readonly struct Cell(int id, Vector2 iv, int cellWidth, int cellHeight)
{
    public readonly int Id = id;
    public readonly int X = (int)iv.X * cellWidth;
    public readonly int Y = (int)iv.Y * cellHeight;
}