using System.Collections.Concurrent;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    
    public static class Collections
    {
        public static readonly Grid Grid = new (Game.MAP_WIDTH,Game.MAP_HEIGHT,500,500);
        
        // NoN-cOnStAnT fIeLdS sHoUld NoT bE vIsIbLe
        // public static Entity[] EntitiesArray;
    }
}