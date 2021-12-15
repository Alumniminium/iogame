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
        public const int FoodEnd = 999_999;
        public const int NpcStart = 1_000_000;
        public const int NpcEnd = 1_999_999;
        public const int PlayerStart = 2_000_000;
        public const int PlayerEnd = 2_999_999;
        public const int BulletStart = 3_000_000;

        public static int Get<T>() => AvailableIds[typeof(T)].Dequeue();

        public static void Recycle(ShapeEntity entity) => AvailableIds[entity.GetType()].Enqueue(entity.Entity.EntityId);
    }
}