using System.Collections.Immutable;
using System.Numerics;
using iogame.Simulation.Entities;

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
        public List<ShapeEntity> Entities = new();

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
            Bottom =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();
            
            v = iv + new Vector2(-1, 0);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Left =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, 0);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            Right =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, -1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            TopRight =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, -1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            TopLeft =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(1, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            BottomRight =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();

            v = iv + new Vector2(-1, 1);
            i = (int)(v.X + g.Width / g.CellWidth * v.Y);
            BottomLeft =  (i >= 0 && i < g.Width / g.CellWidth * (g.Height / g.CellHeight)) ? g.Cells[i] : new Cell();
        }

        public void Add(ShapeEntity entity)
        {
            if (entity is Player)
                Players++;

            Entities.Add(entity);
        }
        public bool Remove(ShapeEntity entity)
        {
            if (entity is Player)
                Players--;

            return Entities.Remove(entity);
        }
        public void Clear()
        {
            Players = 0;
            Entities.Clear();
        }
    }
}