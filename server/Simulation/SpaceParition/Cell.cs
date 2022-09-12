using System.Collections.Generic;
using System.Numerics;
using server.ECS;

namespace iogame.Simulation
{
    // Todo:
    /*
        * I need to make a Cell class that can have flags like "Contains Players" so I can avoid (re)spawning new units where players can see it.
        * Can we put a grid inside the grid so we can grid while we grid?
        * Might want to stop the instert/clear on each tick and move objects between cells?
    */
    public class Cell
    {
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
            var v = iv + new Vector2(0, -1);
            var i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Top = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(0, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Bottom = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, 0);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Left = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, 0);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Right = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, -1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            TopRight = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, -1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            TopLeft = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            BottomRight = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            BottomLeft = (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();
        }

        public void Add(PixelEntity entity)
        {
            if (entity.IsPlayer())
                Players++;

            Entities.Add(entity);
        }
        public void Remove(PixelEntity entity)
        {
            if (entity.IsPlayer())
                Players--;

            Entities.Remove(entity);
        }
        public void Clear()
        {
            Players = 0;
            Entities.Clear();
        }
    }
}