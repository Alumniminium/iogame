using System;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public class AsteroidNeighborTrackingSystem : NttSystem<DeathTagComponent, AsteroidNeighborComponent>
{
    public AsteroidNeighborTrackingSystem() : base("Asteroid Neighbor Tracking", threads: 1) { }
    public override void Update(in NTT ntt, ref DeathTagComponent death, ref AsteroidNeighborComponent neighbors)
    {
        // Update each neighbor when this block dies
        UpdateNeighborReference(neighbors.North, Direction.South);
        UpdateNeighborReference(neighbors.South, Direction.North);
        UpdateNeighborReference(neighbors.East, Direction.West);
        UpdateNeighborReference(neighbors.West, Direction.East);
    }

    private void UpdateNeighborReference(NTT neighbor, Direction fromDirection)
    {
        if (neighbor.Id == Guid.Empty) return;

        ref var neighborRefs = ref neighbor.Get<AsteroidNeighborComponent>();

        // Clear the reference to the dead block
        neighborRefs.ClearDirection(fromDirection);
        neighborRefs.NeighborCount--;

        // All blocks already have physics bodies, so no need to create new ones
        // The neighbor tracking is mainly used for structural integrity calculations

        // Mark for structural integrity recalculation
        if (neighbor.Has<StructuralIntegrityComponent>())
        {
            ref var integrity = ref neighbor.Get<StructuralIntegrityComponent>();
            integrity.NeedsRecalculation = true;
        }
    }

    private static uint GetBlockColor(AsteroidBlockComponent blockData)
    {
        // Default asteroid block color - could be enhanced to use block type
        return 0xFF808080; // Gray
    }
}