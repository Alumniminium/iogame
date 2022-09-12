using System;
using System.Collections.Generic;
using System.Linq;
using server.Simulation.Entities;

namespace server.Helpers
{
    public static class IdGenerator
    {
        private static readonly Dictionary<Type, Queue<int>> AvailableIds = new()
        {
            [typeof(ShapeEntity)] = new Queue<int>(Enumerable.Range(FoodStart, FoodEnd)),
            [typeof(Bullet)] = new Queue<int>(Enumerable.Range(BulletStart, BulletEnd)),
            [typeof(Boid)] = new Queue<int>(Enumerable.Range(NpcStart, NpcEnd)),
            [typeof(Player)] = new Queue<int>(Enumerable.Range(PlayerStart, PlayerEnd)),
            [typeof(Structure)] = new Queue<int>(Enumerable.Range(StructureStart, StructureEnd)),
            [typeof(Drop)] = new Queue<int>(Enumerable.Range(DropStart, DropEnd)),
            [typeof(Asteroid)] = new Queue<int>(Enumerable.Range(AsteroidStart, AsteroidEnd)),
        };
        public const int FoodStart = 0;
        public const int FoodEnd = 250_000;
        public const int PlayerStart = 250_001;
        public const int PlayerEnd = 251_000;
        public const int NpcStart = 251_001;
        public const int NpcEnd = 252_000;
        public const int StructureStart = 252_001;
        public const int StructureEnd = 255_000;
        public const int DropStart = 255_001;
        public const int DropEnd = 265_000;
        public const int AsteroidStart = 265_001;
        public const int AsteroidEnd = 275_000;
        public const int BulletStart = 275_001;
        public const int BulletEnd = 300_000;

        public static int Get<T>()
        {
            return AvailableIds[typeof(T)].Dequeue();
        }

        public static void Recycle(ShapeEntity ntt)
        {
            AvailableIds[ntt.GetType()].Enqueue(ntt.Entity.Id);
        }
    }
}