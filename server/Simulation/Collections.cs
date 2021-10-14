using System.Collections.Concurrent;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public static class Collections
    {
        public static readonly Grid Grid = new (Game.MAP_WIDTH,Game.MAP_HEIGHT,1000,1000);

        public static readonly ConcurrentDictionary<uint, Player> Players = new();
        public static readonly ConcurrentDictionary<uint, Entity> Entities = new();
        
        public static Entity[] EntitiesArray;

        public static List<Entity> EntitiesToRemove = new ();
        public static List<Entity> EntitiesToAdd = new ();

    }
}