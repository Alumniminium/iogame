using System;
using System.Numerics;

namespace server.Simulation.SpaceParition
{
    public readonly struct Cell
    {
        public readonly int X;
        public readonly int Y;

        public Cell(Grid g, Vector2 iv)
        {
            X = (int)iv.X * g.CellWidth;
            Y = (int)iv.Y * g.CellHeight;
        }
        public override bool Equals(object obj) => obj is Cell cell && X == cell.X && Y == cell.Y;
        public static bool operator ==(Cell a, Cell b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Cell a, Cell b) => !(a == b);

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}