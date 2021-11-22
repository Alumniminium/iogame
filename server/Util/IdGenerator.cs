using System;
using iogame.Simulation;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class IdGenerator
    {
        public const int FOOD_START = 0;
        public const int FOOD_END = 99_999;
        public const int NPC_START = 100_000_000;
        public const int NPC_END = 999_999_999;
        public const int PLAYER_START = 1_000_000;
        public const int PLAYER_END = 9_999_999;
        public const int BULLET_START = 10_000_000;
        public const int BULLET_END = 99_999_999;
        private static int lastPlayerId = PLAYER_START;
        private static int lastBulletId = BULLET_START;
        private static int lastNPCId = NPC_START;
        private static int lastFoodId = FOOD_START;

        private static int LastFoodId
        {
            get => lastFoodId;
            set => lastFoodId = value > FOOD_END ? FOOD_START : value;
        }
        private static int LastNPCId
        {
            get => lastNPCId;
            set => lastNPCId = value > NPC_END ? NPC_START : value;
        }
        private static int LastPlayerId
        {
            get => lastPlayerId;
            set => lastPlayerId = value > PLAYER_END ? PLAYER_START : value;
        }
        private static int LastBulletId
        {
            get => lastBulletId;
            set => lastBulletId = value > BULLET_END ? BULLET_START : value;
        }

        public static int Get<T>()
        {
            if (typeof(T) == typeof(Player))
                return LastPlayerId += 1;
            if (typeof(T) == typeof(BaseResource))
                return LastFoodId += 1;
            if (typeof(T) == typeof(Bullet))
                return LastBulletId += 1;
            if(typeof(T) == typeof(Boid))
                return LastNPCId += 1;

            return LastNPCId += 1;
        }
    }
}