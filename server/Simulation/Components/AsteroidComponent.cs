using server.ECS;

namespace server.Simulation.Components;

public enum Direction
{
    North,
    South,
    East,
    West
}

[Component]
public struct AsteroidBlockComponent
{
    public int AsteroidId;
    public bool IsAnchor;      // Core blocks that provide support
    public bool HasPhysics;     // Currently has Box2D body
}

[Component]
public struct AsteroidNeighborComponent
{
    public NTT North, South, East, West;
    public int NeighborCount;  // Quick edge detection

    // Helper methods
    public readonly bool IsEdge => NeighborCount < 4;

    public void ClearDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: North = default; break;
            case Direction.South: South = default; break;
            case Direction.East: East = default; break;
            case Direction.West: West = default; break;
        }
    }
}

[Component]
public struct StructuralIntegrityComponent
{
    public int SupportDistance;     // Distance to nearest anchor
    public float Integrity;         // 0-1, visual cracking
    public bool NeedsRecalculation;
}

// Tag component for collapse
[Component]
public struct StructuralCollapseComponent { }

public enum AsteroidBlockType
{
    Stone = 0,
    IronOre = 1,
    CopperOre = 2,
    RareOre = 3
}