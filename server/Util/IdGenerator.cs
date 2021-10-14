using System;
using iogame.Simulation;
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
            set
            {
                if (value > FOOD_END)
                    lastFoodId = FOOD_START;
                else
                    lastFoodId = value;
            }
        }
        private static uint LastNPCId
        {
            get => lastNPCId;
            set
            {
                if (value > NPC_END)
                    lastNPCId = NPC_START;
                else
                    lastNPCId = value;
            }
        }
        private static uint LastPlayerId
        {
            get => lastPlayerId;
            set
            {
                if (value > PLAYER_END)
                    lastPlayerId = PLAYER_START;
                else
                    lastPlayerId = value;
            }
        }
        private static uint LastBulletId
        {
            get => lastBulletId;

            set
            {
                if (value > BULLET_END)
                    lastBulletId = BULLET_START;
                else
                    lastBulletId = value;
            }
        }

        public static uint Get<T>()
        {
            if (typeof(T) == typeof(Player))
                return LastPlayerId += 1;
            if (typeof(T) == typeof(RedTriangle) || typeof(T) == typeof(YellowSquare) || typeof(T) == typeof(PurpleOctagon) || typeof(T) == typeof(PurplePentagon))
                return LastFoodId += 1;
            if (typeof(T) == typeof(Bullet))
                return LastBulletId += 1;

            return LastNPCId += 1;
        }
    }
}