using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.SpaceParition
{
    public class Cell
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int Players;
        public bool HasPlayers => Players > 0;
        public List<PixelEntity> Entities = new();

        public Cell Top;
        public Cell Bottom;
        public Cell Left;
        public Cell Right;
        public Cell TopLeft;
        public Cell TopRight;
        public Cell BottomLeft;
        public Cell BottomRight;


        public void Init(Grid g, Vector2 iv)
        {
            X = (int)iv.X * g.CellWidth;
            Y = (int)iv.Y * g.CellHeight;
            Width = g.CellWidth;
            Height = g.CellHeight;
            
            var v = iv + new Vector2(0, -1);
            var i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Top = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(0, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Bottom = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, 0);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Left = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, 0);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Right = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, -1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            TopRight = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, -1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            TopLeft = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            BottomRight = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            BottomLeft = i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight) ? g.Cells[i] : new Cell();
        }

        public void Add(in PixelEntity entity)
        {
            if (entity.Type == EntityType.Player)
                Players++;

            lock(Entities)
            Entities.Add(entity);
        }
        public void Remove(in PixelEntity entity)
        {
            if (entity.Type == EntityType.Player)
                Players--;

            lock(Entities)
            Entities.Remove(entity);
        }
        public void Clear()
        {
            Players = 0;
            // lock(Entities)
            Entities.Clear();
        }
    }
}