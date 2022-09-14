using System.Collections.Generic;
using System.Linq;
using server.ECS;

namespace server.Helpers
{
    public static class IdGenerator
    {
        private static readonly Dictionary<EntityType, Queue<int>> AvailableIds = new()
        {
            [EntityType.Passive] = new Queue<int>(Enumerable.Range(PassiveStart, PassiveEnd)),
            [EntityType.Projectile] = new Queue<int>(Enumerable.Range(ProjectileStart, ProjectileEnd)),
            [EntityType.Npc] = new Queue<int>(Enumerable.Range(NpcStart, NpcEnd)),
            [EntityType.Player] = new Queue<int>(Enumerable.Range(PlayerStart, PlayerEnd)),
            [EntityType.Static] = new Queue<int>(Enumerable.Range(StaticStart, StaticEnd)),
            [EntityType.Pickable] = new Queue<int>(Enumerable.Range(PickableStart, PickableEnd)),
        };
        public const int PassiveStart = 0;
        public const int PassiveEnd = 250_000;
        public const int PlayerStart = 250_001;
        public const int PlayerEnd = 251_000;
        public const int NpcStart = 251_001;
        public const int NpcEnd = 252_000;
        public const int StaticStart = 252_001;
        public const int StaticEnd = 255_000;
        public const int PickableStart = 255_001;
        public const int PickableEnd = 265_000;
        public const int ProjectileStart = 275_001;
        public const int ProjectileEnd = 300_000;

        public static int Get(EntityType type) => AvailableIds[type].Dequeue();

        public static void Recycle(in PixelEntity ntt) => AvailableIds[ntt.Type].Enqueue(ntt.Id);
    }
}