using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using server.Helpers;
using server.Simulation.Systems;

namespace server.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 250_000;

        private static readonly PixelEntity[] Entities;
        public static readonly PixelSystem[] Systems;
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly ConcurrentDictionary<int, int> EntityToArrayOffset = new();
        private static readonly Stack<PixelEntity> ToBeRemoved = new();
        private static readonly Stack<PixelEntity> ChangedEntities = new();
        public static readonly HashSet<PixelEntity> Players = new();
        public static readonly HashSet<PixelEntity> ChangedThisTick = new();

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(1, MaxEntities - 1));
            var systems = new List<PixelSystem>
            {
                new SpawnSystem(),
                new LifetimeSystem(),
                new ViewportSystem(),
                new BoidSystem(),
                new InputSystem(),
                new EnergySystem(),
                new ShieldSystem(),
                new WeaponSystem(),
                new EngineSystem(),
                new PhysicsSystem(),
                new CollisionDetector(),
                new PickupCollisionResolver(),
                new BodyDamageResolver(),
                new ProjectileCollisionSystem(),
                new DamageSystem(),
                new HealthSystem(),
                new DropSystem(),
                new DeathSystem(),
                new LevelExpSystem(),
                new RespawnSystem(),
                new NetSyncSystem(),
                new CleanupSystem()
            };
            Systems = systems.ToArray();
        }

        public static ref PixelEntity CreateEntity(EntityType type)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryPop(out var arrayIndex))
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
        public static void InformChangesFor(in PixelEntity ntt)
        {
            if (!ChangedThisTick.Contains(ntt))
            {
                ChangedThisTick.Add(ntt);
                ChangedEntities.Push(ntt);
            }
        }
        public static void Destroy(in PixelEntity ntt) => ToBeRemoved.Push(ntt);
        private static void DestroyInternal(in PixelEntity ntt)
        {
            if (EntityToArrayOffset.TryRemove(ntt.Id, out var arrayOffset))
                AvailableArrayIndicies.Push(arrayOffset);

            Players.Remove(ntt);
            OutgoingPacketQueue.Remove(in ntt);
            IncomingPacketQueue.Remove(in ntt);
            ntt.Recycle();
            ChangedEntities.Push(ntt);
        }
        public static void Update()
        {
            while (ToBeRemoved.Count != 0)
                DestroyInternal(ToBeRemoved.Pop());
            while (ChangedEntities.Count != 0)
            {
                var ntt = ChangedEntities.Pop();
                foreach (var system in Systems)
                    system.EntityChanged(in ntt);
            }
            ChangedThisTick.Clear();
        }
    }
}