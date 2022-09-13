using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using server.Helpers;
using server.Simulation;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 500_000;

        private static readonly PixelEntity[] Entities;
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly ConcurrentDictionary<int, int> EntityToArrayOffset = new();
        private static readonly ConcurrentDictionary<PixelEntity, List<PixelEntity>> Children = new();
        private static readonly ConcurrentStack<PixelEntity> ToBeRemoved = new();
        private static readonly ConcurrentStack<PixelEntity> ChangedEntities = new();

        public static readonly RefList<PixelEntity> Players = new();
        public static readonly RefList<PixelSystem> Systems = new();

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(0, MaxEntities));
        }

        public static ref PixelEntity CreateEntity(EntityType type)
        {
            var id = IdGenerator.Get(type);
            // FConsole.WriteLine($"Creating {id}... Total Entities: {MaxEntities - AvailableArrayIndicies.Count}");
            var ntt = new PixelEntity(id, type);
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                EntityToArrayOffset.TryAdd(ntt.Id, arrayIndex);
                Entities[arrayIndex] = ntt;
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("No more space in array");
        }
        public static ref PixelEntity GetEntity(int nttId)
        {
            return ref Entities[EntityToArrayOffset[nttId]];
        }

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
        public static bool EntityExists(int nttId)
        {
            return EntityToArrayOffset.ContainsKey(nttId);
        }

        public static bool EntityExists(in PixelEntity ntt)
        {
            return EntityToArrayOffset.ContainsKey(ntt.Id);
        }

        public static void InformChangesFor(in PixelEntity ntt) => ChangedEntities.Push(ntt);

        public static void Destroy(in PixelEntity ntt) => ToBeRemoved.Push(ntt);

        private static void DestroyInternal(in PixelEntity ntt)
        {
            if (!EntityExists(in ntt))
                return;

            Game.Grid.Remove(in ntt);
            IdGenerator.Recycle(in ntt);
            Players.Remove(in ntt);
            OutgoingPacketQueue.Remove(in ntt);
            IncomingPacketQueue.Remove(in ntt);

            if (!EntityToArrayOffset.TryGetValue(ntt.Id, out var arrayOffset))
                return;

            EntityToArrayOffset.TryRemove(ntt.Id, out var _);
            AvailableArrayIndicies.Push(arrayOffset);

            ntt.Recycle();
        }
        public static void Update(bool endOfFrame)
        {
            if (endOfFrame)
                while (ToBeRemoved.TryPop(out var ntt))
                {
                    DestroyInternal(ntt);
                    for (var j = 0; j < Systems.Count; j++)
                        Systems[j].EntityChanged(in ntt);
                }

            while (ChangedEntities.TryPop(out var ntt))
                for (var j = 0; j < Systems.Count; j++)
                    Systems[j].EntityChanged(in ntt);
        }
    }
}