using System;
using System.Collections.Generic;
using System.Linq;
using server.Helpers;
using server.Simulation;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 100_000;

        private static readonly PixelEntity[] Entities;
        private static readonly List<int> ChangedEntities = new();
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly Dictionary<int, int> EntityToArrayOffset;
        private static readonly Dictionary<int, List<int>> Children = new();
        private static readonly Dictionary<PixelEntity, ShapeEntity> EntitiyToShapeEntitiy = new();
        private static readonly List<int> ToBeRemoved = new();

        public static readonly Dictionary<int, Player> Players = new();
        public static readonly Dictionary<int, ShapeEntity> ShapeEntities = new();
        public static readonly List<PixelSystem> Systems;
        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new Stack<int>(Enumerable.Range(0, MaxEntities));
            EntityToArrayOffset = new Dictionary<int, int>();
            Systems = new List<PixelSystem>();
        }

        public static ref PixelEntity CreateEntity(int id)
        {
            Console.WriteLine($"Creating {id}... Total Entities: " + ShapeEntities.Count);
            var entity = new PixelEntity
            {
                EntityId = id
            };
            var arrayIndex = AvailableArrayIndicies.Pop();
            EntityToArrayOffset.TryAdd(entity.EntityId, arrayIndex);
            Entities[arrayIndex] = entity;
            InformChangesFor(entity.EntityId);
            return ref Entities[arrayIndex];
        }

        public static List<int> GetChildren(ref PixelEntity entity)
        {
            if (!Children.ContainsKey(entity.EntityId))
                Children.Add(entity.EntityId, new List<int>());
            return Children[entity.EntityId];
        }
        public static void AddChildFor(ref PixelEntity entity, ref PixelEntity child)
        {
            child.Parent = entity.EntityId;
            if (!Children.ContainsKey(entity.EntityId))
                Children.Add(entity.EntityId, new List<int>());
            else
                Children[entity.EntityId].Add(child.EntityId);
        }
        internal static void AttachEntityToShapeEntity(PixelEntity ecsEntity, ShapeEntity gameEntity)
        {
            EntitiyToShapeEntitiy.Add(ecsEntity, gameEntity);
            if (!ShapeEntities.ContainsKey(gameEntity.Entity.EntityId))
                ShapeEntities.Add(gameEntity.Entity.EntityId, gameEntity);
        }
        internal static ShapeEntity GetAttachedShapeEntity(ref PixelEntity ecsEntity) => EntitiyToShapeEntitiy[ecsEntity];

        public static bool EntityExists(int entityId) => EntityToArrayOffset.ContainsKey(entityId);
        public static ref PixelEntity GetEntity(int entityId) => ref Entities[EntityToArrayOffset[entityId]];

        public static void InformChangesFor(int entityId)
        {
            if (ChangedEntities.Contains(entityId))
                return;

            ChangedEntities.Add(entityId);
            ref var entity = ref GetEntity(entityId);

            foreach (var childId in entity.Children)
                ChangedEntities.Add(childId);
        }
        public static void Destroy(int id)
        {
            if (ToBeRemoved.Contains(id))
                return;
            ToBeRemoved.Add(id);
        }
        private static void DestroyInternal(int id)
        {
            FConsole.WriteLine($"Destroying {id}... Total Entities: " + ShapeEntities.Count);
            if (EntityToArrayOffset.TryGetValue(id, out var arrayOffset))
            {
                ref var entity = ref Entities[arrayOffset];
                for (int i = 0; i < Systems.Count; i++)
                    Systems[i].EntityChanged(ref entity);

                foreach (var childId in entity.Children)
                {
                    ref var child = ref GetEntity(childId);
                    DestroyInternal(child.EntityId);
                }
                var shapeEntity = GetAttachedShapeEntity(ref entity);
                IdGenerator.Recycle(shapeEntity);
                if(entity.Has<ColliderComponent>())
                    Game.Tree.Remove(entity.Get<ColliderComponent>());
                EntitiyToShapeEntitiy.Remove(entity);
                ShapeEntities.Remove(entity.EntityId);
                Players.Remove(entity.EntityId);

                entity.Recycle();
                AvailableArrayIndicies.Push(arrayOffset);
            }
        }
        public static void Update()
        {
            foreach (var id in ToBeRemoved)
                DestroyInternal(id);

            foreach (var entityId in ChangedEntities)
            {
                ref var entity = ref GetEntity(entityId);
                for (int i = 0; i < Systems.Count; i++)
                    Systems[i].EntityChanged(ref entity);
            }

            ChangedEntities.Clear();
            ToBeRemoved.Clear();
        }
    }
}