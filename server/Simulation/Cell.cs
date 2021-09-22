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

        public List<Entity> Entities = new();

        public Cell()
        {
        }

        public void Add(Entity entity)
        {
            if(entity is Player)
               Players++;

            Entities.Add(entity);
        }
        public void Remove(Entity entity)
        {
            if(entity is Player)
               Players--;

            Entities.Remove(entity);
        }
        public void Clear()
        {
            Players = 0;
            Entities.Clear();
        }
    }
}