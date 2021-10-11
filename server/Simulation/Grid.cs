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


        public Dictionary<Vector2, Cell> Cells = new();

        public Grid(int mapWidth, int mapHeight, int cellWidth, int cellHeight)
        {
            Width = mapWidth;
            Height = mapHeight;
            CellWidth = cellWidth;
            CellHeight = cellHeight;

            for (int x = -CellWidth; x < mapWidth+cellWidth; x += cellWidth)
                for (int y = -CellHeight; y < mapHeight+cellHeight; y += cellHeight)
                    Cells.Add(new Vector2(x / cellWidth, y / cellHeight), new Cell());
        }

        // Adds an entity to the grid and puts it in the correct cell
        public void Insert(Entity entity)
        {
            var cell = GetCell(entity);
            cell.Add(entity);
        }


        // Removes an entity from the cell
        public void Remove(Entity entity)
        {
            var cell = GetCell(entity);
            cell.Remove(entity);
        }

        public void Move(Vector2 oldPosition, Entity entity)
        {
            if(oldPosition == entity.Position)
                return;
                
            var vector = FindGridIdx(oldPosition);
            var newVextor = FindGridIdx(entity);

            if (vector == newVextor)
                return;

            Cells[vector].Remove(entity);
            Cells[newVextor].Add(entity);
        }

        /// Doesn't actually remove Cells, just their contents.
        public void Clear()
        {
            foreach (var kvp in Cells)
                kvp.Value.Clear();
        }

        // Returns all the entities in the cell of the entity and all cells he's moving towards
        public IEnumerable<Entity> GetEntitiesSameAndDirection(Entity entity)
        {
            var returnList = new List<Vector2>();

            var entityMoveDir = entity.Velocity.Unit();
            entityMoveDir.X = (int)Math.Round(entityMoveDir.X, 0);
            entityMoveDir.Y = (int)Math.Round(entityMoveDir.Y, 0);

            var vector = FindGridIdx(entity);
            returnList.Add(vector);
            returnList.Add(entityMoveDir);

            if (entityMoveDir.X == -1) // moving left
            {
                returnList.Add(vector + new Vector2(-1, 0));  // left
                returnList.Add(vector + new Vector2(-1, 1));  // top left
                returnList.Add(vector + new Vector2(-1, -1)); // bottom left
            }
            else if (entityMoveDir.X == 1)
            {
                returnList.Add(vector + new Vector2(1, 0));  // right
                returnList.Add(vector + new Vector2(1, 1));  // top right
                returnList.Add(vector + new Vector2(1, -1)); // bottom right
            }
            if (entityMoveDir.Y == -1)
            {
                returnList.Add(vector + new Vector2(0, -1));  // bottom
                returnList.Add(vector + new Vector2(-1, -1)); // bottom left
                returnList.Add(vector + new Vector2(1, -1));  // bottom right
            }
            else if (entityMoveDir.Y == 1)
            {
                returnList.Add(vector + new Vector2(0, 1));  // top
                returnList.Add(vector + new Vector2(-1, 1));  // top left     
                returnList.Add(vector + new Vector2(1, 1));  // top right
            }

            for (int i = 0; i < returnList.Count; i++)
            {
                var vect = returnList[i];
                var cell = Cells[vect];

                for (int j = 0; j < cell.Entities.Count; j++)
                    yield return cell.Entities[j];
            }
        }

        // Returns all the entities in the cell of the player and all cells surrounding it
        public IEnumerable<Entity> GetEntitiesSameAndSurroundingCells(Entity entity)
        {
            var returnList = new Vector2[9];
            var vector = FindGridIdx(entity);

            returnList[0]= vector;
            returnList[1]= vector + new Vector2(1, 0);    //
            returnList[2]= vector + new Vector2(0, 1);    //
            returnList[3]= vector + new Vector2(1, 1);    //
            returnList[4]= vector + new Vector2(-1, 0);   // There has to be a better way
            returnList[5]= vector + new Vector2(0, -1);   //
            returnList[6]= vector + new Vector2(-1, -1);  //
            returnList[7]= vector + new Vector2(1, -1);   //
            returnList[8]= vector + new Vector2(-1, 1);   //

            for (int i = 0; i < returnList.Length; i++)
            {
                var vect = returnList[i];
                var cell = Cells[vect];

                for (int j = 0; j < cell.Entities.Count; j++)
                    yield return cell.Entities[j];
            }
        }

        // returns all entities in the cell
        public List<Entity> GetEntitiesSameCell(Entity entity)
        {
            var cell = GetCell(entity);
            return cell.Entities;
        }

        public Vector2 FindGridIdx(Entity e)
        {
            var x = (int)e.Position.X;
            var y = (int)e.Position.Y;

            x /= CellWidth;
            y /= CellHeight;

            return new(x,y);
        }

        public Vector2 FindGridIdx(Vector2 v)
        {
            var x = (int)v.X;
            var y = (int)v.Y;

            x /= CellWidth;
            y /= CellHeight;

            return new(x,y);
        }

        private Cell GetCell(Entity entity)
        {
            var vector = FindGridIdx(entity);
            return Cells[vector];
        }
    }
}