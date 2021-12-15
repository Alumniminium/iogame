using System.Collections.Concurrent;
using server.Helpers;
using server.Simulation;
using server.Simulation.Entities;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 1_000_000;

        private static readonly PixelEntity[] Entities;
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly Dictionary<int, int> EntityToArrayOffset;
        private static readonly Dictionary<PixelEntity, List<PixelEntity>> Children = new();
        private static readonly Dictionary<PixelEntity, ShapeEntity> EntitiyToShapeEntitiy = new();
        private static readonly List<PixelEntity> ToBeRemoved = new();
        private static readonly List<PixelEntity> ChangedEntities = new();

        public static readonly List<PixelEntity> Players = new();
        public static readonly List<PixelEntity> Bullets = new();
        public static readonly List<PixelEntity> Structures = new();
        public static readonly List<PixelEntity> Npcs = new();
        public static readonly List<PixelEntity> Resources = new();
        public static readonly List<PixelSystem> Systems;

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(0, MaxEntities));
            EntityToArrayOffset = new();
            Systems = new List<PixelSystem>();
        }

        public static ref PixelEntity CreateEntity(int id)
        {
            // FConsole.WriteLine($"Creating {id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");
            var entity = new PixelEntity(id);
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                EntityToArrayOffset.TryAdd(entity.EntityId, arrayIndex);
                Entities[arrayIndex] = entity;
                InformChangesFor(in entity);
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("No more space in array");
        }
        public static ref PixelEntity GetEntity(int entityId) => ref Entities[EntityToArrayOffset[entityId]];

        public static List<PixelEntity> GetChildren(in PixelEntity entity)
        {
            if (!Children.ContainsKey(entity))
                Children.TryAdd(entity, new List<PixelEntity>());
            return Children[entity];
        }
        public static void AddChildFor(in PixelEntity entity, in PixelEntity child)
        {
            Children.TryAdd(entity, new List<PixelEntity>());
            Children[entity].Add(child);
        }
        internal static void AttachEntityToShapeEntity(in PixelEntity ecsEntity, ShapeEntity gameEntity)
        {
            EntitiyToShapeEntitiy.TryAdd(ecsEntity, gameEntity);

            // switch (gameEntity.GetType())
            // {

            // }
        }
        internal static ShapeEntity GetAttachedShapeEntity(in PixelEntity ecsEntity) => EntitiyToShapeEntitiy[ecsEntity];

        public static bool EntityExists(int entityId) => EntityToArrayOffset.ContainsKey(entityId);
        public static bool EntityExists(in PixelEntity entity) => EntityToArrayOffset.ContainsKey(entity.EntityId);

        public static void InformChangesFor(in PixelEntity entity) => ChangedEntities.Add(entity);
        public static void Destroy(in PixelEntity entity) => ToBeRemoved.Add(entity);

        private static void DestroyInternal(in PixelEntity entity)
        {
            FConsole.WriteLine($"Destroying {entity}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");

            ChangedEntities.Remove(entity);
            Players.Remove(entity);
            Resources.Remove(entity);
            Structures.Remove(entity);
            Bullets.Remove(entity);
            Npcs.Remove(entity);
            OutgoingPacketQueue.Remove(in entity);
            IncomingPacketQueue.Remove(in entity);

            if (!EntityToArrayOffset.TryGetValue(entity.EntityId, out var arrayOffset))
                return;
                
            EntityToArrayOffset.Remove(entity.EntityId);
            AvailableArrayIndicies.Push(arrayOffset);

            foreach (var child in entity.Children)
                DestroyInternal(in child);

            var shapeEntity = GetAttachedShapeEntity(in entity);
            IdGenerator.Recycle(shapeEntity);

            if (Game.Tree.Contains(shapeEntity))
                lock (Game.Tree)
                    Game.Tree.Remove(shapeEntity);

            entity.Recycle();

            for (var i = 0; i < Systems.Count; i++)
                Systems[i].EntityChanged(in entity);
        }
        public static void Update()
        {
            for (var j = 0; j < ToBeRemoved.Count; j++)
                DestroyInternal(ToBeRemoved[j]);

            for (var i = 0; i < ChangedEntities.Count; i++)
            {
                var entity = ChangedEntities[i];
                for (var j = 0; j < Systems.Count; j++)
                    Systems[j].EntityChanged(in entity);
            }

            ChangedEntities.Clear();
            ToBeRemoved.Clear();
        }
    }
}