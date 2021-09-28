using System.Collections.Concurrent;
using System.Numerics;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public static class Collections
    {
        public static ConcurrentDictionary<uint, Player> Players = new();
        public static ConcurrentDictionary<uint, Entity> Entities = new();
        public static Grid Grid = new Grid();

        public static List<(Vector2, Entity)> MovementsTickTick = new List<(Vector2, Entity)>();
        public static Entity[] EntitiesArray;
    }
}