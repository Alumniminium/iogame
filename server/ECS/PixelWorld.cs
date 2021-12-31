using System.Collections.Concurrent;
using server.Helpers;
using server.Simulation;
using server.Simulation.Entities;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 100_000_000;

        private static readonly PixelEntity[] Entities;
        private static readonly ShapeEntity[] ShapeEntities;
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly Dictionary<int, int> EntityToArrayOffset = new();
        private static readonly Dictionary<PixelEntity, List<PixelEntity>> Children = new();
        private static readonly ConcurrentStack<PixelEntity> ToBeRemoved = new();
        private static readonly ConcurrentStack<PixelEntity> ChangedEntities = new();

        public static readonly RefList<PixelEntity> Players = new();
        public static readonly RefList<PixelEntity> Bullets = new();
        public static readonly RefList<PixelEntity> Structures = new();
        public static readonly RefList<PixelEntity> Npcs = new();
        public static readonly RefList<PixelSystem> Systems = new();

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            ShapeEntities = new ShapeEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(0, MaxEntities));
        }

        public static ref PixelEntity CreateEntity(int id)
        {
            FConsole.WriteLine($"Creating {id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");
            var ntt = new PixelEntity(id);
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                EntityToArrayOffset.TryAdd(ntt.Id, arrayIndex);
                Entities[arrayIndex] = ntt;
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("No more space in array");
        }
        public static ref PixelEntity GetEntity(int nttId) => ref Entities[EntityToArrayOffset[nttId]];

        public static List<PixelEntity> GetChildren(in PixelEntity ntt)
        {
            Children.TryAdd(ntt, new List<PixelEntity>());
            return Children[ntt];
        }
        public static void AddChildFor(in PixelEntity ntt, in PixelEntity child)
        {
            var children = GetChildren(in ntt);
            children.Add(child);
        }
        internal static void AttachEntityToShapeEntity(in PixelEntity ntt, ShapeEntity gameEntity)
        {
            InformChangesFor(in ntt);
            ShapeEntities[ntt.Id] = gameEntity;
            switch (gameEntity)
            {
                case Player:
                    {
                        Players.Add(ntt);
                        break;
                    }
                case Boid:
                    {
                        Npcs.Add(ntt);
                        break;
                    }
                case Bullet:
                    {
                        Bullets.Add(ntt);
                        break;
                    }
                case Structure:
                    {
                        Structures.Add(ntt);
                        break;
                    }
            }
        }
        internal static ref ShapeEntity GetAttachedShapeEntity(in PixelEntity ecsEntity) => ref ShapeEntities[ecsEntity.Id];
        public static bool EntityExists(int nttId) => EntityToArrayOffset.ContainsKey(nttId);
        public static bool EntityExists(in PixelEntity ntt) => EntityToArrayOffset.ContainsKey(ntt.Id);
        public static void InformChangesFor(in PixelEntity ntt) => ChangedEntities.Push(ntt);
        public static void Destroy(in PixelEntity ntt) => ToBeRemoved.Push(ntt);

        private static void DestroyInternal(in PixelEntity ntt)
        {
            FConsole.WriteLine($"Destroying {ntt.Id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");

            if (!EntityExists(in ntt))
                return;

            ref var shapeEntity = ref GetAttachedShapeEntity(in ntt);
            if(shapeEntity != null)
            {
                Game.Tree.Remove(shapeEntity);
                IdGenerator.Recycle(shapeEntity);
            }

            Players.Remove(ntt);
            Structures.Remove(ntt);
            Bullets.Remove(ntt);
            Npcs.Remove(ntt);
            OutgoingPacketQueue.Remove(in ntt);
            IncomingPacketQueue.Remove(in ntt);

            if (!EntityToArrayOffset.TryGetValue(ntt.Id, out var arrayOffset))
                return;

            EntityToArrayOffset.Remove(ntt.Id);
            AvailableArrayIndicies.Push(arrayOffset);

            foreach (var child in ntt.Children)
                DestroyInternal(in child);

            ntt.Recycle();
        }
        public static void Update()
        {
            while (ToBeRemoved.TryPop(out var ntt))
                DestroyInternal(ntt);
            
            while(ChangedEntities.TryPop(out var ntt))
            {
                FConsole.WriteLine($"Updating {ntt.Id}");
                ParallelOptions options = new () { MaxDegreeOfParallelism = Systems.Count / 2 };
                Parallel.For(0,Systems.Count,options, j => Systems[j].EntityChanged(in ntt));
                // for(int j = 0; j < Systems.Count; j++)
                //     Systems[j].EntityChanged(in ntt);
                Thread.Yield();
            }
        }
    }
}