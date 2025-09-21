using System;
using System.Collections.Generic;
using System.Numerics;
using server.Simulation.Components;

namespace server.Helpers;

public static class AsteroidGenerator
{
    private static int _nextAsteroidId = 1;

    public struct AsteroidBlock
    {
        public Vector2 Position;
        public AsteroidBlockType BlockType;
        public uint Color;
        public int Health;
        public int DropAmount;
    }

    public static AsteroidBlock[] GenerateAsteroid(Vector2 center, int radius, Vector2 hollowSize, int seed = 0)
    {
        var random = seed == 0 ? Random.Shared : new Random(seed);
        var blocks = new List<AsteroidBlock>();

        var hollowHalfWidth = hollowSize.X / 2f;
        var hollowHalfHeight = hollowSize.Y / 2f;

        // Create FastNoise instances for different scales
        var baseNoise = new FastNoiseLite(seed);
        baseNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        baseNoise.SetFrequency(0.02f);

        var detailNoise = new FastNoiseLite(seed + 1000);
        detailNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        detailNoise.SetFrequency(0.08f);

        var roughNoise = new FastNoiseLite(seed + 2000);
        roughNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        roughNoise.SetFrequency(0.15f);

        for (int y = -radius - 10; y <= radius + 10; y++)
        {
            for (int x = -radius - 10; x <= radius + 10; x++)
            {
                // Check if point is in hollow area (spawn zone)
                if (Math.Abs(x) <= hollowHalfWidth && Math.Abs(y) <= hollowHalfHeight)
                    continue;

                var distance = MathF.Sqrt(x * x + y * y);

                // Base circular falloff
                var baseDensity = 1f - (distance / radius);
                if (baseDensity <= 0)
                    continue;

                // Get noise values at different scales
                var baseNoiseValue = baseNoise.GetNoise(x, y) * 0.4f;      // Large scale shape variation
                var detailNoiseValue = detailNoise.GetNoise(x, y) * 0.3f;  // Medium scale details
                var roughNoiseValue = roughNoise.GetNoise(x, y) * 0.2f;    // Small scale roughness

                // Combine all noise
                var totalNoise = baseNoiseValue + detailNoiseValue + roughNoiseValue;

                // Final density calculation
                var finalDensity = baseDensity + totalNoise;

                // Threshold for block placement
                if (finalDensity < 0.3f)
                    continue;

                var worldPos = new Vector2(center.X + x, center.Y + y);
                var blockType = DetermineBlockType(x, y, random);
                var color = GetBlockColor(blockType);
                var health = GetBlockHealth(blockType);
                var dropAmount = GetDropAmount(blockType);

                blocks.Add(new AsteroidBlock
                {
                    Position = worldPos,
                    BlockType = blockType,
                    Color = color,
                    Health = health,
                    DropAmount = dropAmount
                });
            }
        }

        return blocks.ToArray();
    }

    public static int GetNextAsteroidId()
    {
        return _nextAsteroidId++;
    }


    private static AsteroidBlockType DetermineBlockType(int x, int y, Random random)
    {
        var distanceFromOrigin = MathF.Sqrt(x * x + y * y);
        var roll = random.NextSingle();

        // Rare ores closer to center
        if (distanceFromOrigin < 3 && roll < 0.05f)
            return AsteroidBlockType.RareOre;

        // Iron ore scattered throughout
        if (roll < 0.15f)
            return AsteroidBlockType.IronOre;

        // Copper ore
        if (roll < 0.25f)
            return AsteroidBlockType.CopperOre;

        // Default to stone
        return AsteroidBlockType.Stone;
    }

    private static uint GetBlockColor(AsteroidBlockType blockType)
    {
        return blockType switch
        {
            AsteroidBlockType.Stone => 0xFF808080,      // Gray
            AsteroidBlockType.IronOre => 0xFF8B4513,    // Brown
            AsteroidBlockType.CopperOre => 0xFFCD7F32,  // Copper
            AsteroidBlockType.RareOre => 0xFF9400D3,    // Purple
            _ => 0xFF808080
        };
    }

    private static int GetBlockHealth(AsteroidBlockType blockType)
    {
        return blockType switch
        {
            AsteroidBlockType.Stone => 20,
            AsteroidBlockType.IronOre => 30,
            AsteroidBlockType.CopperOre => 25,
            AsteroidBlockType.RareOre => 50,
            _ => 20
        };
    }

    private static int GetDropAmount(AsteroidBlockType blockType)
    {
        return blockType switch
        {
            AsteroidBlockType.Stone => 1,
            AsteroidBlockType.IronOre => 2,
            AsteroidBlockType.CopperOre => 2,
            AsteroidBlockType.RareOre => 5,
            _ => 1
        };
    }
}