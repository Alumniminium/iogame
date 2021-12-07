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
            [typeof(Bullet)] = new Queue<int>(Enumerable.Range(BulletStart, BulletStart*2)),
            [typeof(Boid)] = new Queue<int>(Enumerable.Range(NpcStart, NpcEnd)),
            [typeof(Player)] = new Queue<int>(Enumerable.Range(PlayerStart, PlayerEnd)),
        };
        public const int FoodStart = 1;
        public const int FoodEnd = 99_999;
        public const int NpcStart = 100_000;
        public const int NpcEnd = 199_999;
        public const int PlayerStart = 200_000;
        public const int PlayerEnd = 299_999;
        public const int BulletStart = 300_000;

        public static int Get<T>() => AvailableIds[typeof(T)].Dequeue();

        public static void Recycle(ShapeEntity entity) => AvailableIds[entity.GetType()].Enqueue(entity.Entity.EntityId);
    }
}