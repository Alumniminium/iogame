using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class IdGenerator
    {
        public static Dictionary<Type, Queue<int>> AvailableIds = new()
        {
            [typeof(ShapeEntity)] = new Queue<int>(Enumerable.Range(FOOD_START, FOOD_END)),
            [typeof(Bullet)] = new Queue<int>(Enumerable.Range(BULLET_START, BULLET_START*2)),
            [typeof(Boid)] = new Queue<int>(Enumerable.Range(NPC_START, NPC_END)),
            [typeof(Player)] = new Queue<int>(Enumerable.Range(PLAYER_START, PLAYER_END)),
        };
        public const int FOOD_START = 1;
        public const int FOOD_END = 19_999;
        public const int NPC_START = 20_000;
        public const int NPC_END = 39_999;
        public const int PLAYER_START = 40_000;
        public const int PLAYER_END = 59_999;
        public const int BULLET_START = 60_000;

        public static int Get<T>() => AvailableIds[typeof(T)].Dequeue();

        public static void Recycle(ShapeEntity entity) => AvailableIds[entity.GetType()].Enqueue(entity.Entity.EntityId);
    }
}