using System.Numerics;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Grid
    {
        public const int W = 300;
        public const int H = 300;
        public Dictionary<Vector2, Cell> Cells = new();

        private List<Entity> emptyList = new List<Entity>(); 

        // Adds an entity to the grid and puts it in the correct cell
        public void Insert(Entity entity)
        {
            var vector = FindGridIdx(entity);

            if (!Cells.TryGetValue(vector, out var cell))
                Cells.Add(vector, new Cell());
            Cells[vector].Add(entity);
        }

        // Removes an entity from the cell
        public void Remove(Entity entity)
        {
            var vector = FindGridIdx(entity);
            Cells[vector].Remove(entity);
        }

        /// Doesn't actually remove Cells, just their contents.
        public void Clear()
        {
            foreach (var kvp in Cells)
                kvp.Value.Clear();
        }

        // Returns all the entities in the cell of the player and all cells surrounding it
        public IEnumerable<List<Entity>> GetEntitiesSameAndSurroundingCells(Entity entity)
        {
            var vectors = new List<Vector2>(); // Todo don't allocate

            var vector = FindGridIdx(entity);
            vectors.Add(vector); //28,6
            vectors.Add(vector + new Vector2(1, 0));    //
            vectors.Add(vector + new Vector2(0, 1));    //
            vectors.Add(vector + new Vector2(1, 1));    //
            vectors.Add(vector + new Vector2(-1, 0));   // There has to be a better way
            vectors.Add(vector + new Vector2(0, -1));   //
            vectors.Add(vector + new Vector2(-1, -1));  //
            vectors.Add(vector + new Vector2(1, -1));   //
            vectors.Add(vector + new Vector2(-1, 1));   //

            foreach (var v in vectors)
            {
                if (Cells.TryGetValue(v, out var cell))
                        yield return cell.Entities;
                else
                    Cells.Add(v, new Cell());
            }
        }

        // returns all entities in the cell
        public List<Entity> GetEntitiesSameCell(Entity entity)
        {
            var vector = FindGridIdx(entity);

            if (Cells.TryGetValue(vector, out var cell))
                return cell.Entities;
            else
                Cells.Add(vector, new Cell());
                
            return emptyList;
        }

        private static Vector2 FindGridIdx(Entity e) => new Vector2(((int)e.Position.X) / W, ((int)e.Position.Y) / H);
    }
}