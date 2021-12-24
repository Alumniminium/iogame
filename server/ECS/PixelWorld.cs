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
        public const int MaxEntities = 1_000_000;

        private static readonly PixelEntity[] Entities;
        private static readonly Queue<int> AvailableArrayIndicies;
        private static readonly Dictionary<int, int> EntityToArrayOffset;
        private static readonly Dictionary<PixelEntity, List<PixelEntity>> Children = new();
        private static readonly Dictionary<PixelEntity, ShapeEntity> EntitiyToShapeEntitiy = new();
        private static readonly List<PixelEntity> ToBeRemoved = new();
        private static readonly List<PixelEntity> ChangedEntities = new();

        public static readonly List<PixelEntity> Players = new();
        public static readonly List<PixelEntity> Bullets = new();
        public static readonly List<PixelEntity> Structures = new();
        public static readonly List<PixelEntity> Npcs = new();
        public static readonly List<PixelEntity> Triangles = new();
        public static readonly List<PixelEntity> Squares = new();
        public static readonly List<PixelEntity> Pentagons = new();
        public static readonly List<PixelEntity> Hexagons = new();
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
            if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
            {
                EntityToArrayOffset.TryAdd(entity.Id, arrayIndex);
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
        internal static void AttachEntityToShapeEntity(in PixelEntity entity, ShapeEntity gameEntity)
        {
            EntitiyToShapeEntitiy.TryAdd(entity, gameEntity);
            switch (gameEntity)
            {
                case Player p:
                    {
                        Players.Add(entity);
                        break;
                    }
                case Boid b:
                    {
                        Npcs.Add(entity);
                        break;
                    }
                case Bullet b:
                    {
                        Bullets.Add(entity);
                        break;
                    }
                case Structure s:
                    {
                        Structures.Add(entity);
                        break;
                    }
                case ShapeEntity e:
                    {
                        ref readonly var shp = ref entity.Get<ShapeComponent>();
                        switch (shp.Sides)
                        {
                            case 3:
                                Triangles.Add(entity);
                                break;
                            case 4:
                                Squares.Add(entity);
                                break;
                            case 5:
                                Pentagons.Add(entity);
                                break;
                            case 6:
                                Hexagons.Add(entity);
                                break;
                        }
                        break;
                    }
            }
        }
        internal static ShapeEntity GetAttachedShapeEntity(in PixelEntity ecsEntity) => EntitiyToShapeEntitiy[ecsEntity];
        internal static bool HasAttachedShapeEntity(in PixelEntity ecsEntity) => EntitiyToShapeEntitiy.ContainsKey(ecsEntity);

        public static bool EntityExists(int entityId) => EntityToArrayOffset.ContainsKey(entityId);
        public static bool EntityExists(in PixelEntity entity) => EntityToArrayOffset.ContainsKey(entity.Id);

        public static void InformChangesFor(in PixelEntity entity)
        {
            if (!ChangedEntities.Contains(entity))
                ChangedEntities.Add(entity);
        }

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
            Triangles.Remove(entity);
            Squares.Remove(entity);
            Pentagons.Remove(entity);
            Hexagons.Remove(entity);
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