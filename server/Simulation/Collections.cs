using System.Collections.Concurrent;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public static class Collections
    {
        public static readonly ConcurrentDictionary<uint, Player> Players = new();
        public static readonly ConcurrentDictionary<uint, Entity> Entities = new();
        public static readonly Grid Grid = new ();
        public static Entity[] EntitiesArray;
    }
}