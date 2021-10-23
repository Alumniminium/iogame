using System.Numerics;
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

        // Adds an entity to the grid and puts it in the correct cell
        public void Insert(Entity entity)
        {
            var cell = FindCell(entity);
            cell.Add(entity);
        }


        // Removes an entity from the cell
        public void Remove(Entity entity)
        {
            var cell = FindCell(entity);
            cell.Remove(entity);
        }

        public void Move(Entity entity)
        {
            (Vector2 pos, Vector2 lastPos, float _) = entity.PositionComponent;

            if (pos == lastPos)
                return;

            var cell = FindCell(lastPos);
            var newCell = FindCell(pos);

            if (cell == newCell)
                return;

            cell.Remove(entity);
            newCell.Add(entity);
        }

        /// Doesn't actually remove Cells, just their contents.
        public void Clear()
        {
            foreach (var cell in Cells)
                cell.Clear();
        }

        // Returns all the entities in the cell of the entity and all cells he's moving towards
        public IEnumerable<Entity> GetEntitiesSameAndDirection(Entity entity)
        {
            var returnList = new List<Cell>();

            var entityMoveDir = entity.VelocityComponent.Movement.Unit();
            if (entityMoveDir.X > 0)
                entityMoveDir.X = 1;
            else if (entityMoveDir.X < 0)
                entityMoveDir.X = -1;

            if (entityMoveDir.Y > 0)
                entityMoveDir.Y = 1;
            else if (entityMoveDir.Y < 0)
                entityMoveDir.Y = -1;

            var cell = FindCell(entity);

            returnList.Add(cell);

            if (entityMoveDir.X == -1)
            {
                returnList.Add(cell.Left);
                returnList.Add(cell.TopLeft);
                returnList.Add(cell.BottomLeft);
            }
            else if (entityMoveDir.X == 1)
            {
                returnList.Add(cell.Right);
                returnList.Add(cell.TopRight);
                returnList.Add(cell.BottomRight);
            }
            if (entityMoveDir.Y == -1)
            {
                returnList.Add(cell.Bottom);
                returnList.Add(cell.BottomLeft);
                returnList.Add(cell.BottomRight);
            }
            else if (entityMoveDir.Y == 1)
            {
                returnList.Add(cell.Top);
                returnList.Add(cell.TopLeft);
                returnList.Add(cell.TopRight);
            }

            for (int i = 0; i < returnList.Count; i++)
            {
                var c = returnList[i];
                for (int j = 0; j < c.Entities.Count; j++)
                    yield return c.Entities[j];
            }
        }

        // Returns all the entities in the cell of the player and all cells surrounding it
        public IEnumerable<Entity> GetEntitiesSameAndSurroundingCells(Entity entity)
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

        // returns all entities in the cell
        public List<Entity> GetEntitiesSameCell(Entity entity) => FindCell(entity).Entities;

        public IEnumerable<Entity> GetEntitiesInViewport(Entity entity)
        {
            (Vector2 pos,_, float _) = entity.PositionComponent;
            for (var x = pos.X - Entity.VIEW_DISTANCE; x < pos.X + Entity.VIEW_DISTANCE - CellWidth; x += CellWidth)
                for (var y = pos.Y - Entity.VIEW_DISTANCE; y < pos.Y + Entity.VIEW_DISTANCE - CellHeight; y += CellHeight)
                {
                    var cell = FindCell(new Vector2(x, y));

                    if (cell == null)
                        continue;
                    foreach (var e in cell.Entities)
                        yield return e;
                }
        }

        public Cell FindCell(Entity e) => FindCell(e.PositionComponent.Position);

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