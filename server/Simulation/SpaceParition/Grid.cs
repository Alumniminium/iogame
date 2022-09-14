using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.SpaceParition
{
    public sealed class Grid
    {
        public int EntityCount;
        public readonly int Width;
        public readonly int Height;
        public readonly int CellWidth;
        public readonly int CellHeight;
        public Cell[] Cells;
        public ConcurrentDictionary<PixelEntity, Cell> EntityCells = new();
        public ConcurrentDictionary<Cell, List<PixelEntity>> CellEntities = new();

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
                    Cells[(int)(iv.X + Width / cellWidth * iv.Y)] = new Cell(this, iv);
                }
        }

        // Adds an entity to the grid and puts it in the correct cell
        public void Add(in PixelEntity entity, ref PhysicsComponent phy)
        {
            var cell = FindCell(phy.Position);
            EntityCells.TryAdd(entity, cell);
            if(!CellEntities.TryGetValue(cell, out var list))
            {
                list = new List<PixelEntity>();
                CellEntities.TryAdd(cell, list);
            }
            lock(list)
            list.Add(entity);
            EntityCount++;
        }

        // Removes an entity from the cell
        public bool Remove(in PixelEntity entity)
        {
            if (!EntityCells.TryRemove(entity, out var cell))
                return false;
            if (!CellEntities.TryGetValue(cell, out var list))
                return false;
            lock(list)
            return list.Remove(entity);
        }

        public void Move(in PixelEntity entity)
        {
            if(Remove(in entity))
                EntityCount--;
            ref var phy = ref entity.Get<PhysicsComponent>();
            Add(entity, ref phy);
        }

        public void GetVisibleEntities(ref ViewportComponent vwp)
        {
            var rect = vwp.Viewport;
            var topLeft = new Vector2(rect.X, rect.Y);
            var bottomRight = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

            topLeft = Vector2.Clamp(topLeft, Vector2.Zero, new Vector2(Width - 1, Height - 1));
            bottomRight = Vector2.Clamp(bottomRight, Vector2.Zero, new Vector2(Width - 1, Height - 1));

            var start = FindCell(in topLeft);
            var end = FindCell(in bottomRight);
            List<PixelEntity> entities = new();
            for (int x = start.X; x <= end.X; x += CellWidth)
                for (int y = start.Y; y <= end.Y; y += CellHeight)
                {
                    var cell = FindCell(new Vector2(x, y));
                    if (CellEntities.TryGetValue(cell, out var list))
                        entities.AddRange(list);
                }
            vwp.EntitiesVisible = entities.ToArray();
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