using iogame.ECS;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation.Managers
{
    public static class World
    {
        public static int EntityCount => 50_000 - AvailableArrayIndicies.Count;
        private static readonly Entity[] Entities;
        private static readonly List<int> ChangedEntities = new();
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly Dictionary<int, int> EntityToArrayOffset, UniqueIdToEntityId, EntityIdToUniqueId;
        private static readonly Dictionary<int, List<int>> Children = new();
        private static readonly Dictionary<Entity, ShapeEntity> EntitiyToShapeEntitiy = new();
        private static readonly List<int> ToBeRemoved = new();

        public static readonly Dictionary<int, Player> Players = new();
        public static readonly Dictionary<int, ShapeEntity> ShapeEntities = new();
        private static readonly List<Entity> ToBeAdded = new();
        public static List<PixelSystem> Systems;
        static World()
        {
            Entities = new Entity[50_001];
            AvailableArrayIndicies = new Stack<int>(Enumerable.Range(1, 50_000));
            EntityToArrayOffset = new Dictionary<int, int>();
            UniqueIdToEntityId = new Dictionary<int, int>();
            EntityIdToUniqueId = new Dictionary<int, int>();
            Systems = new List<PixelSystem>();
        }

        public static T GetSystem<T>() where T : PixelSystem
        {
            for (int i = 0; i < Systems.Count; i++)
            {
                var sys = Systems[i];
                if (sys is T type)
                    return type;
            }
            throw new ArgumentException("No system of requested type");
        }

        private static void RegisterUniqueIdFor(int uniqueId, int entityId)
        {
            UniqueIdToEntityId.TryAdd(uniqueId, entityId);
            EntityIdToUniqueId.TryAdd(entityId, uniqueId);
        }

        public static ref Entity CreateEntity(int id)
        {
            var entity = new Entity
            {
                EntityId = id
            };
            var arrayIndex = AvailableArrayIndicies.Pop();
            EntityToArrayOffset.TryAdd(entity.EntityId, arrayIndex);
            Entities[arrayIndex] = entity;
            InformChangesFor(entity.EntityId);
            RegisterUniqueIdFor(id, entity.EntityId);
            return ref Entities[arrayIndex];
        }

        public static List<int> GetChildren(ref Entity entity)
        {
            if (!Children.ContainsKey(entity.EntityId))
                Children.Add(entity.EntityId, new List<int>());
            return Children[entity.EntityId];
        }
        public static void AddChildFor(ref Entity entity, ref Entity child)
        {
            child.Parent = entity.EntityId;
            if (!Children.TryGetValue(entity.EntityId, out var children))
                Children.Add(entity.EntityId, new List<int>());
            else
                Children[entity.EntityId].Add(child.EntityId);
        }
        internal static void AttachEntityToShapeEntity(Entity ecsEntity, ShapeEntity gameEntity)
        {
            EntitiyToShapeEntitiy.Add(ecsEntity, gameEntity);
            if (!ShapeEntities.ContainsKey(gameEntity.EntityId))
                ShapeEntities.Add(gameEntity.EntityId, gameEntity);
        }
        internal static ShapeEntity GetAttachedShapeEntity(Entity ecsEntity)
        {
            EntitiyToShapeEntitiy.TryGetValue(ecsEntity, out var shape);
            return shape;
        }

        public static bool EntityExists(int entityId) => EntityToArrayOffset.ContainsKey(entityId);

        public static Entity[] GetEntities() => Entities;
        public static List<int> GetChangedEntities() => ChangedEntities;
        public static ref Entity GetEntity(int entityId) => ref Entities[EntityToArrayOffset[entityId]];
        public static ref Entity GetEntityByUniqueId(int uniqueId) => ref Entities[EntityToArrayOffset[UniqueIdToEntityId[uniqueId]]];

        public static void InformChangesFor(int entityId)
        {
            if (ChangedEntities.Contains(entityId))
                return;

            ChangedEntities.Add(entityId);
            ref var entity = ref GetEntity(entityId);

            if (entity.Children != null)
                foreach (var childId in entity.Children)
                    ChangedEntities.Add(childId);
        }
        public static bool UidExists(int uid) => UniqueIdToEntityId.ContainsKey(uid);
        public static void Destroy(int id)
        {
            if (ToBeRemoved.Contains(id))
                return;
            ToBeRemoved.Add(id);
        }
        private static void DestroyInternal(int id)
        {
            Console.WriteLine("World.DestroyInternal");
            if (EntityToArrayOffset.TryGetValue(id, out var arrayOffset))
            {
                ref var entity = ref Entities[arrayOffset];
                if (entity.Children != null)
                {
                    foreach (var childId in entity.Children)
                    {
                        ref var child = ref GetEntity(childId);
                        DestroyInternal(child.EntityId);
                    }
                }
                var shapeEntity = GetAttachedShapeEntity(entity);
                if (shapeEntity is Player player)
                    OutgoingPacketQueue.Remove(player);

                CollisionDetection.Grid.Remove(shapeEntity);
                shapeEntity.Viewport.Clear();
                EntitiyToShapeEntitiy.Remove(entity);
                ShapeEntities.Remove(entity.EntityId);
                Players.Remove(entity.EntityId);

                entity.Recycle();
                AvailableArrayIndicies.Push(arrayOffset);
            }
            EntityIdToUniqueId.Remove(id, out var uid);
            UniqueIdToEntityId.Remove(uid, out _);
        }
        private static void AddEntityInternal(Entity entity)
        {
            // if (shapeEntity is Player player)
            //     Players.TryAdd(entity.EntityId, player);

            // Entities.TryAdd(entity.EntityId, entity);
        }
        public static void Update()
        {
            foreach (var id in ToBeRemoved)
                DestroyInternal(id);

            foreach (var entity in ToBeAdded)
                AddEntityInternal(entity);

            foreach (var entityId in ChangedEntities)
            {
                ref var entity = ref GetEntity(entityId);
                for (int i = 0; i < Systems.Count; i++)
                    Systems[i].EntityChanged(ref entity);
            }

            ChangedEntities.Clear();
            ToBeRemoved.Clear();
            ToBeAdded.Clear();
        }
    }
}