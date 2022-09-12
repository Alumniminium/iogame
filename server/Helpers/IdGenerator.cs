using System.Collections.Generic;
using System.Linq;
using server.ECS;

namespace server.Helpers
{
    public static class IdGenerator
    {
        private static readonly Dictionary<EntityType, Queue<int>> AvailableIds = new()
        {
            [EntityType.Food] = new Queue<int>(Enumerable.Range(FoodStart, FoodEnd)),
            [EntityType.Bullet] = new Queue<int>(Enumerable.Range(BulletStart, BulletEnd)),
            [EntityType.Boid] = new Queue<int>(Enumerable.Range(NpcStart, NpcEnd)),
            [EntityType.Player] = new Queue<int>(Enumerable.Range(PlayerStart, PlayerEnd)),
            [EntityType.Structure] = new Queue<int>(Enumerable.Range(StructureStart, StructureEnd)),
            [EntityType.Drop] = new Queue<int>(Enumerable.Range(DropStart, DropEnd)),
            [EntityType.Asteroid] = new Queue<int>(Enumerable.Range(AsteroidStart, AsteroidEnd)),
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

        public static int Get(EntityType type) => AvailableIds[type].Dequeue();

        public static void Recycle(PixelEntity ntt) => AvailableIds[ntt.Type].Enqueue(ntt.Id);
    }
}