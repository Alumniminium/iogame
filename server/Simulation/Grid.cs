using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Grid
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // Adds an entity to the grid and puts it in the correct cell
        public void Insert(ShapeEntity entity)
        {
            var cell = FindCell(entity);
            cell.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // Removes an entity from the cell
        public void Remove(ShapeEntity entity)
        {
            var cell = FindCell(entity);
            if (!cell.Remove(entity))
            {
                foreach (var c in Cells)
                    if (c.Remove(entity))
                        break;
            } 
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Move(ShapeEntity entity)
        {
            var pos = entity.PositionComponent.Position;
            var lastPos = entity.PositionComponent.LastPosition;

            if (pos == lastPos)
                return;

            var cell = FindCell(lastPos);
            var newCell = FindCell(pos);

            if (cell == newCell)
                return;

            cell.Remove(entity);
            newCell.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        /// Doesn't actually remove Cells, just their contents.
        public void Clear()
        {
            for (int i = 0; i < Cells.Length; i++)
                Cells[i].Clear();
            // Cells.AsParallel().ForAll((i)=> i.Clear()); // twice as fast, gc not measured
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // Returns all the entities in the cell of the player and all cells surrounding it
        public IEnumerable<ShapeEntity> GetEntitiesSameAndSurroundingCells(ShapeEntity entity)
        {
            var returnList = new Cell[9];
            var cell = FindCell(entity);

            returnList[0] = cell;
            returnList[1] = cell.Left;   //
            returnList[2] = cell.Right;   //
            returnList[3] = cell.Top;   //
            returnList[4] = cell.Bottom;   // There has to be a better way
            returnList[5] = cell.TopLeft;   //
            returnList[6] = cell.TopRight;   //
            returnList[7] = cell.BottomLeft;   //
            returnList[8] = cell.BottomRight;   //

            for (int i = 0; i < returnList.Length; i++)
            {
                var c = returnList[i];
                for (int j = 0; j < c.Entities.Count; j++)
                    yield return c.Entities[j];
            }
        }

        public static List<ShapeEntity> returnList = new(1000);
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public List<ShapeEntity> GetEntitiesSameAndSurroundingCellsList(ShapeEntity entity)
        {
            returnList.Clear();
            var cells = new Cell[9];
            var cell = FindCell(entity);

            cells[0] = cell;
            cells[1] = cell.Left;   //
            cells[2] = cell.Right;   //
            cells[3] = cell.Top;   //
            cells[4] = cell.Bottom;   // There has to be a better way
            cells[5] = cell.TopLeft;   //
            cells[6] = cell.TopRight;   //
            cells[7] = cell.BottomLeft;   //
            cells[8] = cell.BottomRight;   //

            for (int i = 0; i < cells.Length; i++)
            {
                var c = cells[i];
                returnList.AddRange(c.Entities);
            }
            return returnList;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // returns all entities in the cell
        public List<ShapeEntity> GetEntitiesSameCell(ShapeEntity entity) => FindCell(entity).Entities;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe List<ShapeEntity> GetEntitiesInViewport(ShapeEntity entity)
        {
            var entities = new List<ShapeEntity>();
            ref readonly var pos = ref entity.PositionComponent.Position;

            for (var x = pos.X - entity.VIEW_DISTANCE; x < pos.X + entity.VIEW_DISTANCE - CellWidth; x += CellWidth)
                for (var y = pos.Y - entity.VIEW_DISTANCE; y < pos.Y + entity.VIEW_DISTANCE - CellHeight; y += CellHeight)
                {
                    var cell = FindCell(new Vector2(x, y));
                    entities.AddRange(cell.Entities);
                }
            return entities;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public Cell FindCell(ShapeEntity e) => FindCell(e.PositionComponent.Position);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public Cell FindCell(Vector2 v)
        {
            var x = (int)v.X;
            var y = (int)v.Y;

            x /= CellWidth;
            y /= CellHeight;
            var cw = Width / CellWidth;

            var i = y * (cw - 1) + x;

            if (i >= Cells.Length || i < 0)
                return Cells[0];

            return Cells[i];
        }
    }
}