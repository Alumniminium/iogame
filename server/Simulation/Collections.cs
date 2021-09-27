using System.Collections.Concurrent;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public static class Collections
    {
        public static ConcurrentDictionary<uint, Player> Players = new();
        public static ConcurrentDictionary<uint, Entity> Entities = new();
        public static Grid Grid = new Grid();

        public static Entity[] EntitiesArray;
    }
}