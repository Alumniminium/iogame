using System.Numerics;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Grid
    {
        public const int W = 1500;
        public const int H = 1500;
        public Dictionary<Vector2, Cell> Cells = new();

        private List<Entity> emptyList = new ();
        
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
            var vector = FindGridIdx(oldPosition);
            var newVextor = FindGridIdx(entity);
            
            if(vector == newVextor)
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

            var entityMoveDir = entity.Velocity.unit();
            entityMoveDir.X = (int)Math.Round(entityMoveDir.X,0);
            entityMoveDir.Y= (int)Math.Round(entityMoveDir.Y,0);

            var vector = FindGridIdx(entity);
            returnList.Add(vector);
            returnList.Add(entityMoveDir); 
            
            if(entityMoveDir.X == -1) // moving left
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
            if(entityMoveDir.Y == -1)
            {
                returnList.Add(vector + new Vector2(0, -1));  // bottom
                returnList.Add(vector + new Vector2(-1, -1)); // bottom left
                returnList.Add(vector + new Vector2(1, -1));  // bottom right
            }
            else if (entityMoveDir.Y == 1)
            {
                returnList.Add(vector + new Vector2(0, 1));  // top
                returnList.Add(vector + new Vector2(-1,1));  // top left     
                returnList.Add(vector + new Vector2(1, 1));  // top right
            }

            for(int i =0; i< returnList.Count; i++)
            {
                var vect = returnList[i];
                if (Cells.TryGetValue(vect, out var cell))
                       for(int j =0; j< cell.Entities.Count; j++)
                            yield return cell.Entities[j];
                else
                    continue;
            }
        }

        // Returns all the entities in the cell of the player and all cells surrounding it
        public IEnumerable<Entity> GetEntitiesSameAndSurroundingCells(Entity entity)
        {
            var returnList = new List<Vector2>();
            var vector = FindGridIdx(entity);

            returnList.Add(vector);
            returnList.Add(vector + new Vector2(1, 0));    //
            returnList.Add(vector + new Vector2(0, 1));    //
            returnList.Add(vector + new Vector2(1, 1));    //
            returnList.Add(vector + new Vector2(-1, 0));   // There has to be a better way
            returnList.Add(vector + new Vector2(0, -1));   //
            returnList.Add(vector + new Vector2(-1, -1));  //
            returnList.Add(vector + new Vector2(1, -1));   //
            returnList.Add(vector + new Vector2(-1, 1));   //

            for(int i =0; i< returnList.Count; i++)
            {
                var vect = returnList[i];
                if (Cells.TryGetValue(vect, out var cell))
                       for(int j =0; j< cell.Entities.Count; j++)
                            yield return cell.Entities[j];
                else
                    continue;
            }
        }

        // returns all entities in the cell
        public List<Entity> GetEntitiesSameCell(Entity entity)
        {
            var cell = GetCell(entity);
            return cell.Entities;
        }

        public static Vector2 FindGridIdx(Entity e) => new (((int)e.Position.X) / W, ((int)e.Position.Y) / H);
        public static Vector2 FindGridIdx(Vector2 v) => new (((int)v.X) / W, ((int)v.Y) / H);

        private Cell GetCell(Entity entity)
        {
            var vector = FindGridIdx(entity);
            if (Cells.TryGetValue(vector, out var cell))
                return cell;

            Cells.Add(vector, new Cell());
            return Cells[vector];
        }
    }
}