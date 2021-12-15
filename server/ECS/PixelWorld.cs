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
        private static readonly ConcurrentDictionary<int, int> EntityToArrayOffset;
        // private static readonly ConcurrentDictionary<PixelEntity, List<PixelEntity>> Children = new();
        private static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> EntitiyToShapeEntitiy = new();
        private static readonly ConcurrentDictionary<PixelEntity, PixelEntity> ToBeRemoved = new();
        private static readonly ConcurrentDictionary<PixelEntity,PixelEntity> ChangedEntities = new();

        public static readonly ConcurrentDictionary<PixelEntity, Player> Players = new();
        public static readonly ConcurrentDictionary<PixelEntity, Player> Bullets = new();
        public static readonly ConcurrentDictionary<PixelEntity, Player> Structures = new();
        public static readonly ConcurrentDictionary<PixelEntity, Player> Npcs = new();
        public static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> Resources = new();
        public static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> Triangles = new();
        public static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> Squares = new();
        public static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> Pentagons = new();
        public static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> Hexagons = new();
        public static readonly ConcurrentDictionary<PixelEntity, ShapeEntity> Octagons = new();
        public static readonly List<PixelSystem> Systems;
        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new Stack<int>(Enumerable.Range(0, MaxEntities));
            EntityToArrayOffset = new();
            Systems = new List<PixelSystem>();
        }

        public static ref PixelEntity CreateEntity(int id)
        {
            // FConsole.WriteLine($"Creating {id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");
            var entity = new PixelEntity(id);
            var arrayIndex = AvailableArrayIndicies.Pop();
            EntityToArrayOffset.TryAdd(entity.EntityId, arrayIndex);
            Entities[arrayIndex] = entity;
            InformChangesFor(in entity);
            return ref Entities[arrayIndex];
        }

        // public static List<PixelEntity> GetChildren(PixelEntity entity)
        // {
        //     if (!Children.ContainsKey(entity))
        //         Children.TryAdd(entity, new List<PixelEntity>());
        //     return Children[entity];
        // }
        // public static void AddChildFor(in PixelEntity entity, ref PixelEntity child)
        // {
        //     child.Parent = entity.EntityId;
        //     Children.TryAdd(entity, new List<PixelEntity>());
        //     Children[entity].Add(child);
        // }
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
        public static ref PixelEntity GetEntity(int entityId) => ref Entities[EntityToArrayOffset[entityId]];

        public static void InformChangesFor(in PixelEntity entity)
        {
            ChangedEntities.TryAdd(entity,entity);
                // foreach (var childId in entity.Children)
                //     ChangedEntities.Add(childId);
        }
        public static void Destroy(in PixelEntity entity) => ToBeRemoved.TryAdd(entity,entity);

        private static void DestroyInternal(in PixelEntity entity)
        {
            FConsole.WriteLine($"Destroying {entity}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");

            if (!EntityToArrayOffset.TryGetValue(entity.EntityId, out var arrayOffset))
                return;

            // foreach (var child in entity.Children)
            //     DestroyInternal(child);

            var shapeEntity = GetAttachedShapeEntity(in entity);
            IdGenerator.Recycle(shapeEntity);

            if (Game.Tree.Contains(shapeEntity))
                lock (Game.Tree)
                    Game.Tree.Remove(shapeEntity);

            Players.TryRemove(entity, out _);

            entity.Recycle();

            for (var i = 0; i < Systems.Count; i++)
                Systems[i].EntityChanged(in entity);
            
            ChangedEntities.TryRemove(entity, out _);
            AvailableArrayIndicies.Push(arrayOffset);
            EntityToArrayOffset.TryRemove(entity.EntityId, out _);
        }
        public static void Update()
        {
            foreach (var (key, val) in ToBeRemoved)
                DestroyInternal(in key);
            
                foreach(var (key,val) in ChangedEntities)
                {
                    for (var j = 0; j < Systems.Count; j++)
                        Systems[j].EntityChanged(in key);
                }
                ChangedEntities.Clear();
                ToBeRemoved.Clear();
        }
    }
}