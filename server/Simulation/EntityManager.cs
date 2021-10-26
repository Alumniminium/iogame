using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation
{
    public static class EntityManager
    {
        public static readonly Dictionary<uint, Player> Players = new();
        public static readonly Dictionary<uint, Entity> Entities = new();

        public static readonly  List<Entity> EntitiesToRemove = new ();
        public static readonly  List<Entity> EntitiesToAdd = new ();


        public static void AddEntity(Entity entity) => EntitiesToAdd.Add(entity);
        public static void RemoveEntity(Entity entity) => EntitiesToRemove.Add(entity);

        public static void Update()
        {
            RemoveEntity_Internal();
            AddEntity_Internal();
        }
        
        public static void AddEntity_Internal()
        {
            foreach (var entity in EntitiesToAdd)
            {
                if (entity is Player player)
                    Players.TryAdd(entity.UniqueId, player);

                Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
            }
            EntitiesToAdd.Clear();
        }
        public static void RemoveEntity_Internal()
        {
            foreach (var entity in EntitiesToRemove)
            {
                if (entity is Player player)
                {
                    OutgoingPacketQueue.Remove(player);
                    Players.Remove(player.UniqueId, out _);
                }
                Entities.Remove(entity.UniqueId, out _);
                Collections.Grid.Remove(entity);
                entity.Viewport.Clear();
            }
            EntitiesToRemove.Clear();
        }
    }
}