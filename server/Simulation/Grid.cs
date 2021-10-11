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

            Cells = new Cell[Width/cellWidth * (Height / cellHeight)];


            for (int x = 0; x < mapWidth; x += cellWidth)
                for (int y = 0; y < mapHeight; y += cellHeight)
                {
                    var iv = new Vector2(x/cellWidth,y/cellHeight);
                    Cells[(int)(iv.X + Width/cellWidth * iv.Y)] = new Cell();
                }
            for (int x = 0; x < mapWidth; x += cellWidth)
                for (int y = 0; y < mapHeight; y += cellHeight)
                {
                    var iv = new Vector2(x/cellWidth,y/cellHeight);
                    Cells[(int)(iv.X + Width/cellWidth * iv.Y)].Init(this,iv);
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

        public void Move(Vector2 oldPosition, Entity entity)
        {
            if(oldPosition == entity.Position)
                return;
                
            var cell = FindCell(oldPosition);
            var newCell = FindCell(entity);

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
            var returnList = new List<List<Entity>>();

            var entityMoveDir = entity.Velocity.Unit();
            if (entityMoveDir.X > 0)
                entityMoveDir.X = 1;
            else if (entityMoveDir.X < 0)
                entityMoveDir.X = -1;

            if (entityMoveDir.Y > 0)
                entityMoveDir.Y = 1;
            else if (entityMoveDir.Y < 0)
                entityMoveDir.Y = -1;

            var cell = FindCell(entity);

            returnList.Add(cell.Entities);
            
            if (entityMoveDir.X == -1) // moving left
            {
                returnList.Add(cell.Left.Entities);  // left
                returnList.Add(cell.TopLeft.Entities);  // top left
                returnList.Add(cell.BottomLeft.Entities); // bottom left
            }
            else if (entityMoveDir.X == 1)
            {
                returnList.Add(cell.Right.Entities);
                returnList.Add(cell.TopRight.Entities);
                returnList.Add(cell.BottomRight.Entities);
            }
            if (entityMoveDir.Y == -1)
            {
                returnList.Add(cell.Bottom.Entities);
                returnList.Add(cell.BottomLeft.Entities);
                returnList.Add(cell.BottomRight.Entities);
            }
            else if (entityMoveDir.Y == 1)
            {
                returnList.Add(cell.Top.Entities);
                returnList.Add(cell.TopLeft.Entities);
                returnList.Add(cell.TopRight.Entities);
            }

            for (int i = 0; i < returnList.Count; i++)
            {
                var entities = returnList[i];
                for (int j = 0; j < entities.Count; j++)
                    yield return entities[j];
            }
        }

        // Returns all the entities in the cell of the player and all cells surrounding it
        public IEnumerable<Entity> GetEntitiesSameAndSurroundingCells(Entity entity)
        {
            var returnList = new List<Entity>[9];
            var cell = FindCell(entity);

            returnList[0]= cell.Entities;
            returnList[1]= cell.Left.Entities;   //
            returnList[2]= cell.Right.Entities;   //
            returnList[3]= cell.Top.Entities;   //
            returnList[4]= cell.Bottom.Entities;   // There has to be a better way
            returnList[5]= cell.TopLeft.Entities;   //
            returnList[6]= cell.TopRight.Entities;   //
            returnList[7]= cell.BottomLeft.Entities;   //
            returnList[8]= cell.BottomRight.Entities;   //

            for (int i = 0; i < returnList.Length; i++)
            {
                var entities = returnList[i];
                for (int j = 0; j < entities.Count; j++)
                    yield return entities[j];
            }
        }

        // returns all entities in the cell
        public List<Entity> GetEntitiesSameCell(Entity entity) => FindCell(entity).Entities;

        public Cell FindCell(Entity e) => FindCell(e.Position);

        public Cell FindCell(Vector2 v)
        {
            var x = (int)v.X;
            var y = (int)v.Y;

            x /= CellWidth;
            y /= CellHeight;

            return Cells[x+Width/CellWidth*y];
        }
    }
}