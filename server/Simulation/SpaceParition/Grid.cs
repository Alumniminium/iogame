using System;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.SpaceParition
{
    public sealed class Grid
    {
        public readonly int Width;
        public readonly int Height;

        public readonly int CellWidth;
        public readonly int CellHeight;

        public Cell[] Cells;

        public Grid(int mapWidth, int mapHeight, int cellWidth, int cellHeight)
        {
            Width = mapWidth;
            Height = mapHeight;
            CellWidth = cellWidth;
            CellHeight = cellHeight;

            Cells = new Cell[Width / cellWidth * (Height / cellHeight)];


            for (int x = 0; x < mapWidth; x += cellWidth)
                for (int y = 0; y < mapHeight; y += cellHeight)
                {
                    var iv = new Vector2(x / cellWidth, y / cellHeight);
                    Cells[(int)(iv.X + Width / cellWidth * iv.Y)] = new Cell();
                }
            for (int x = 0; x < mapWidth; x += cellWidth)
                for (int y = 0; y < mapHeight; y += cellHeight)
                {
                    var iv = new Vector2(x / cellWidth, y / cellHeight);
                    Cells[(int)(iv.X + Width / cellWidth * iv.Y)].Init(this, iv);
                }
        }

        // Adds an entity to the grid and puts it in the correct cell
        public void Add(in PixelEntity entity)
        {
            ref readonly var phy = ref entity.Get<PhysicsComponent>();
            var cell = FindCell(in phy.Position);
            cell.Add(in entity);
        }


        // Removes an entity from the cell
        public void Remove(in PixelEntity entity)
        {
            ref readonly var phy = ref entity.Get<PhysicsComponent>();
            var cell = FindCell(in phy.Position);
            cell.Remove(in entity);
        }

        public void Move(in PixelEntity entity)
        {
            ref readonly var phy = ref entity.Get<PhysicsComponent>();
            var cell = FindCell(in phy.LastPosition);
            var newCell = FindCell(in phy.Position);

            if (cell == newCell)
                return;

            cell.Remove(in entity);
            newCell.Add(in entity);
        }

        /// Doesn't actually remove Cells, just their contents.
        public void Clear()
        {
            foreach (var cell in Cells)
                cell.Clear();
        }

        public void GetVisibleEntities(in PixelEntity entity)
        {
            ref readonly var vwp = ref entity.Get<ViewportComponent>();
            ref readonly var phy = ref entity.Get<PhysicsComponent>();

            var rect = vwp.Viewport;
            var topLeft = new Vector2(rect.X, rect.Y);
            var bottomRight = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

            topLeft = Vector2.Clamp(topLeft, Vector2.Zero, new Vector2(Width - 1, Height - 1));
            bottomRight = Vector2.Clamp(bottomRight, Vector2.Zero, new Vector2(Width - 1, Height - 1));

            var start = FindCell(in topLeft);
            var end = FindCell(in bottomRight);

            for (int x = start.X; x <= end.X; x += CellWidth)
                for (int y = start.Y; y <= end.Y; y += CellHeight)
                {
                    var cell = FindCell(new Vector2(x, y));
                    vwp.EntitiesVisible.AddRange(cell.Entities);
                }
        }

        public Cell FindCell(in Vector2 v)
        {
            var x = (int)Math.Clamp(v.X, 0, Width - 1);
            var y = (int)Math.Clamp(v.Y, 0, Height - 1);

            x /= CellWidth;
            y /= CellHeight;

            return Cells[x + Width / CellWidth * y];
        }
    }
}