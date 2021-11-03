using System;
using iogame.Simulation;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;

namespace iogame.Util
{
    public static class IdGenerator
    {
        public const uint FOOD_START = 0;
        public const uint FOOD_END = 99_999;
        public const uint NPC_START = 100_000_000;
        public const uint NPC_END = 999_999_999;
        public const uint PLAYER_START = 1_000_000;
        public const uint PLAYER_END = 9_999_999;
        public const uint BULLET_START = 10_000_000;
        public const uint BULLET_END = 99_999_999;
        private static uint lastPlayerId = PLAYER_START;
        private static uint lastBulletId = BULLET_START;
        private static uint lastNPCId = NPC_START;
        private static uint lastFoodId = FOOD_START;

        private static uint LastFoodId
        {
            get => lastFoodId;
            set => lastFoodId = value > FOOD_END ? FOOD_START : value;
        }
        private static uint LastNPCId
        {
            get => lastNPCId;
            set => lastNPCId = value > NPC_END ? NPC_START : value;
        }
        private static uint LastPlayerId
        {
            get => lastPlayerId;
            set => lastPlayerId = value > PLAYER_END ? PLAYER_START : value;
        }
        private static uint LastBulletId
        {
            get => lastBulletId;
            set => lastBulletId = value > BULLET_END ? BULLET_START : value;
        }

        public static uint Get<T>()
        {
            if (typeof(T) == typeof(Player))
                return LastPlayerId += 1;
            if (typeof(T) == typeof(BaseResource))
                return LastFoodId += 1;
            if (typeof(T) == typeof(Bullet))
                return LastBulletId += 1;

            return LastNPCId += 1;
        }
    }
}