using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using server.Helpers;
using server.Simulation;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 500_000;

        private static readonly PixelEntity[] Entities;
        private static readonly ConcurrentQueue<int> AvailableArrayIndicies;
        private static readonly ConcurrentDictionary<int, int> EntityToArrayOffset = new();
        private static readonly ConcurrentQueue<PixelEntity> ToBeRemoved = new();
        private static readonly ConcurrentQueue<PixelEntity> ChangedEntities = new();
        public static readonly List<PixelEntity> Players = new();
        public static readonly List<PixelSystem> Systems = new();

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(0, MaxEntities));
        }

        public static ref PixelEntity CreateEntity(EntityType type)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    var ntt = new PixelEntity(arrayIndex, type);
                    if (EntityToArrayOffset.TryAdd(ntt.Id, arrayIndex))
                    {
                        Entities[arrayIndex] = ntt;
                        return ref Entities[arrayIndex];
                    }
                    throw new Exception("EntityToArrayOffset.TryAdd failed");
                }
                throw new IndexOutOfRangeException("Failed to pop an array index");
            }
            throw new IndexOutOfRangeException("No more space in array");
        }
        public static ref PixelEntity GetEntity(int nttId) => ref Entities[EntityToArrayOffset[nttId]];
        public static bool EntityExists(int nttId) => EntityToArrayOffset.ContainsKey(nttId);
        public static bool EntityExists(in PixelEntity ntt) => EntityToArrayOffset.ContainsKey(ntt.Id);
        public static void InformChangesFor(in PixelEntity ntt) => ChangedEntities.Enqueue(ntt);
        public static void Destroy(in PixelEntity ntt) => ToBeRemoved.Enqueue(ntt);
        private static void DestroyInternal(in PixelEntity ntt)
        {
            if (EntityToArrayOffset.TryRemove(ntt.Id, out var arrayOffset))
                AvailableArrayIndicies.Enqueue(arrayOffset);
            else
                return;

            Players.Remove(ntt);
            OutgoingPacketQueue.Remove(in ntt);
            IncomingPacketQueue.Remove(in ntt);
            ntt.Recycle();
        }
        public static void Update(bool endOfFrame)
        {
            if (endOfFrame)
            {
                while (ToBeRemoved.TryDequeue(out var ntt))
                    DestroyInternal(ntt);
                
                while (ChangedEntities.TryDequeue(out var ntt))
                {
                    if (EntityExists(ntt))
                        for (var j = 0; j < Systems.Count; j++)
                            Systems[j].EntityChanged(ntt);
                    else
                        for (var j = 0; j < Systems.Count; j++)
                            Systems[j]._entities.Remove(ntt.Id);
                }
            }
        }
    }
}