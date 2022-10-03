using System.Numerics;

namespace server.Simulation.SpaceParition
{
    public readonly struct Cell
    {
        public readonly int Id;
        public readonly int X;
        public readonly int Y;

        public Cell(int id, Vector2 iv, int cellWidth, int cellHeight)
        {
            X = (int)iv.X * cellWidth;
            Y = (int)iv.Y * cellHeight;
            Id = id;
        }
    }
}