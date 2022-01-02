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
        };
        public const int FoodStart = 0;
        public const int FoodEnd = 250_000;
        public const int PlayerStart = 250_001;
        public const int PlayerEnd = 251_000;
        public const int NpcStart = 251_001;
        public const int NpcEnd = 252_000;
        public const int BulletStart = 252_001;
        public const int BulletEnd = 300_000;
        
        public static int Get<T>() => AvailableIds[typeof(T)].Dequeue();

        public static void Recycle(ShapeEntity ntt) => AvailableIds[ntt.GetType()].Enqueue(ntt.Entity.Id);
    }
}