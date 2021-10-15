using System.Collections.Concurrent;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    
    public static class Collections
    {
        public static readonly Grid Grid = new (Game.MAP_WIDTH,Game.MAP_HEIGHT,1000,1000);

        public static readonly ConcurrentDictionary<uint, Player> Players = new();
        public static readonly ConcurrentDictionary<uint, Entity> Entities = new();

        public static readonly  List<Entity> EntitiesToRemove = new ();
        public static readonly  List<Entity> EntitiesToAdd = new ();
        
        // NoN-cOnStAnT fIeLdS sHoUld NoT bE vIsIbLe
        public static Entity[] EntitiesArray;
    }
}