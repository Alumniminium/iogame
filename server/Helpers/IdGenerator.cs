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
            [typeof(Structure)] = new Queue<int>(Enumerable.Range(StructureStart,StructureEnd))
        };
        public const int FoodStart = 1;
        public const int FoodEnd = 9_999_999;
        public const int NpcStart = 10_000_000;
        public const int NpcEnd = 19_999_999;
        public const int PlayerStart = 20_000_000;
        public const int PlayerEnd = 29_999_999;
        public const int BulletStart = 30_000_000;
        public const int BulletEnd = 39_999_999;
        public const int StructureStart = 40_000_000;
        public const int StructureEnd = 49_999_999;

        public static int Get<T>() => AvailableIds[typeof(T)].Dequeue();

        public static void Recycle(ShapeEntity ntt) => AvailableIds[ntt.GetType()].Enqueue(ntt.Entity.Id);
    }
}