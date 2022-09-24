using System.Collections.Generic;
using System.Numerics;
using server.ECS;

namespace server.Simulation.SpaceParition
{
    public sealed class Cell
    {
        public readonly int X;
        public readonly int Y;
        public readonly HashSet<PixelEntity> Entities;

        public Cell(Grid g, Vector2 iv)
        {
            X = (int)iv.X * g.CellWidth;
            Y = (int)iv.Y * g.CellHeight;
            Entities = new HashSet<PixelEntity>();
        }
    }
}