using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;
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
        public readonly ConcurrentDictionary<PixelEntity, int> EntityCells = new();
        public readonly ConcurrentDictionary<int, List<PixelEntity>> CellEntities = new();
        public readonly List<PixelEntity> StaticEntities = new();

        public Grid(int mapWidth, int mapHeight, int cellWidth, int cellHeight)
        {
            Width = mapWidth;
            Height = mapHeight;
            CellWidth = cellWidth;
            CellHeight = cellHeight;

            for (int x = 0; x < mapWidth; x += cellWidth)
                for (int y = 0; y < mapHeight; y += cellHeight)
                {
                    var iv = new Vector2(x / cellWidth, y / cellHeight);
                    var id = (int)(iv.X + (Width / cellWidth * iv.Y));

                    CellEntities.TryAdd(id, new List<PixelEntity>());
                }
        }

        // Adds an entity to the grid and puts it in the correct cell
        public void Add(in PixelEntity entity, ref PhysicsComponent phy)
        {
            if (entity.Type == EntityType.Static)
            {
                StaticEntities.Add(entity);
                EntityCount++;
                return;
            }
            var cell = FindCell(phy.Position);
            EntityCells.TryAdd(entity, cell.Id);
            if (!CellEntities.TryGetValue(cell.Id, out var list))
            {
                list = new List<PixelEntity>();
                CellEntities.TryAdd(cell.Id, list);
            }
            // lock (list)
            {
                list.Add(entity);
                EntityCount++;
            }
        }

        // Removes an entity from the cell
        public void Remove(in PixelEntity entity)
        {
            if (entity.Type == EntityType.Static)
            {
                StaticEntities.Remove(entity);
                EntityCount--;
                return;
            }
            if (!EntityCells.TryRemove(entity, out var cell))
                return;
            if (!CellEntities.TryGetValue(cell, out var entities))
                return;
            // lock (entities)
            {
                if (entities.Remove(entity))
                    EntityCount--;
            }
        }

        public void Move(in PixelEntity entity, ref PhysicsComponent phy)
        {
            Remove(in entity);
            Add(in entity, ref phy);
        }

        public void GetVisibleEntities(ref ViewportComponent vwp)
        {
            var rect = vwp.Viewport;
            var topLeft = new Vector2(rect.X, rect.Y);
            var bottomRight = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

            topLeft = Vector2.Clamp(topLeft, Vector2.Zero, new Vector2(Width - 1, Height - 1));
            bottomRight = Vector2.Clamp(bottomRight, Vector2.Zero, new Vector2(Width - 1, Height - 1));

            var start = FindCell(topLeft);
            var end = FindCell(bottomRight);
            var index = 0;
            for (int x = start.X; x <= end.X; x += CellWidth)
                for (int y = start.Y; y <= end.Y; y += CellHeight)
                {
                    var cell = FindCell(new Vector2(x, y));
                    if (CellEntities.TryGetValue(cell.Id, out var list))
                    {
                            vwp.EntitiesVisible.AddRange(list);
                    }
                }
                vwp.EntitiesVisible.AddRange(StaticEntities);
        }

        public Cell FindCell(Vector2 v)
        {
            var v2 = Vector2.Clamp(v, Vector2.Zero, new Vector2(Width - 1, Height - 1));
            var iv = new Vector2((int)(v2.X / CellWidth), (int)(v2.Y / CellHeight));
            var id = (int)(iv.X + (Width / CellWidth * iv.Y));
            return new Cell(id, iv,CellWidth, CellHeight);
        }
    }
}