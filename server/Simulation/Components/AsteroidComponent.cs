using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct AsteroidComponent
{
    public readonly NTT Entity;
    public readonly int AsteroidId;
    public readonly AsteroidBlockType BlockType;

    public AsteroidComponent(NTT entity, int asteroidId, AsteroidBlockType blockType = AsteroidBlockType.Stone)
    {
        Entity = entity;
        AsteroidId = asteroidId;
        BlockType = blockType;
    }
}

public enum AsteroidBlockType
{
    Stone = 0,
    IronOre = 1,
    CopperOre = 2,
    RareOre = 3
}