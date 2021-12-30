using Microsoft.AspNetCore.Mvc.Diagnostics;
using server.Helpers;
using server.Simulation;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 10_000_000;

        private static readonly PixelEntity[] Entities;
        private static readonly Queue<int> AvailableArrayIndicies;
        private static readonly Dictionary<int, int> EntityToArrayOffset = new();
        private static readonly Dictionary<PixelEntity, List<PixelEntity>> Children = new();
        private static readonly Dictionary<PixelEntity, ShapeEntity> EntitiyToShapeEntitiy = new();
        private static readonly HashSet<PixelEntity> ToBeRemoved = new();
        private static readonly HashSet<PixelEntity> ChangedEntities = new();

        public static readonly List<PixelEntity> Players = new();
        public static readonly List<PixelEntity> Bullets = new();
        public static readonly List<PixelEntity> Structures = new();
        public static readonly List<PixelEntity> Npcs = new();
        public static readonly List<PixelSystem> Systems = new();

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(0, MaxEntities));
        }

        public static ref PixelEntity CreateEntity(int id)
        {
            FConsole.WriteLine($"Creating {id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");
            var entity = new PixelEntity(id);
            if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
            {
                EntityToArrayOffset.TryAdd(entity.Id, arrayIndex);
                Entities[arrayIndex] = entity;
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("No more space in array");
        }
        public static ref PixelEntity GetEntity(int entityId) => ref Entities[EntityToArrayOffset[entityId]];

        public static List<PixelEntity> GetChildren(in PixelEntity entity)
        {
            Children.TryAdd(entity, new List<PixelEntity>());
            return Children[entity];
        }
        public static void AddChildFor(in PixelEntity entity, in PixelEntity child)
        {
            var children = GetChildren(in entity);
            children.Add(child);
        }
        internal static void AttachEntityToShapeEntity(in PixelEntity entity, ShapeEntity gameEntity)
        {
            InformChangesFor(in entity);
            EntitiyToShapeEntitiy.TryAdd(entity, gameEntity);
            switch (gameEntity)
            {
                case Player:
                    {
                        Players.Add(entity);
                        break;
                    }
                case Boid:
                    {
                        Npcs.Add(entity);
                        break;
                    }
                case Bullet:
                    {
                        Bullets.Add(entity);
                        break;
                    }
                case Structure:
                    {
                        Structures.Add(entity);
                        break;
                    }
            }
        }
        internal static ShapeEntity GetAttachedShapeEntity(in PixelEntity ecsEntity) => EntitiyToShapeEntitiy[ecsEntity];
        internal static bool HasAttachedShapeEntity(in PixelEntity ecsEntity) => EntitiyToShapeEntitiy.ContainsKey(ecsEntity);
        public static bool EntityExists(int entityId) => EntityToArrayOffset.ContainsKey(entityId);
        public static bool EntityExists(in PixelEntity entity) => EntityToArrayOffset.ContainsKey(entity.Id);
        public static void InformChangesFor(in PixelEntity entity) => ChangedEntities.Add(entity);
        public static void Destroy(in PixelEntity entity) => ToBeRemoved.Add(entity);

        private static void DestroyInternal(in PixelEntity entity)
        {
            FConsole.WriteLine($"Destroying {entity.Id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");

            if (!EntityExists(in entity))
                return;

            if (HasAttachedShapeEntity(in entity))
            {
                var shapeEntity = GetAttachedShapeEntity(in entity);
                Game.Tree.Remove(shapeEntity);
                IdGenerator.Recycle(shapeEntity);
            }

            ChangedEntities.Remove(entity);
            Players.Remove(entity);
            Structures.Remove(entity);
            Bullets.Remove(entity);
            Npcs.Remove(entity);
            OutgoingPacketQueue.Remove(in entity);
            IncomingPacketQueue.Remove(in entity);

            if (!EntityToArrayOffset.TryGetValue(entity.Id, out var arrayOffset))
                return;

            EntityToArrayOffset.Remove(entity.Id);
            AvailableArrayIndicies.Enqueue(arrayOffset);

            foreach (var child in entity.Children)
                DestroyInternal(in child);

            entity.Recycle();

            for (var i = 0; i < Systems.Count; i++)
                Systems[i].EntityChanged(in entity);
        }
        public static void Update()
        {
            foreach (var entity in ToBeRemoved)
                DestroyInternal(entity);

            foreach (var entity in ChangedEntities)
            {
                Parallel.For(0,Systems.Count, j =>
                    Systems[j].EntityChanged(in entity));
            }

            ChangedEntities.Clear();
            ToBeRemoved.Clear();
        }
    }
}