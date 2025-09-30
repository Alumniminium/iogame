using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public class AsteroidNeighborTrackingSystem : NttSystem<DeathTagComponent, AsteroidNeighborComponent>
{
    public AsteroidNeighborTrackingSystem() : base("Asteroid Neighbor Tracking", threads: 1) { }
    public override void Update(in NTT ntt, ref DeathTagComponent death, ref AsteroidNeighborComponent neighbors)
    {
        UpdateNeighborReference(neighbors.North, Direction.South);
        UpdateNeighborReference(neighbors.South, Direction.North);
        UpdateNeighborReference(neighbors.East, Direction.West);
        UpdateNeighborReference(neighbors.West, Direction.East);
    }

    private static void UpdateNeighborReference(NTT neighbor, Direction fromDirection)
    {
        if (neighbor.Id == Guid.Empty) return;

        ref var neighborRefs = ref neighbor.Get<AsteroidNeighborComponent>();

        neighborRefs.ClearDirection(fromDirection);
        neighborRefs.NeighborCount--;

        if (neighbor.Has<StructuralIntegrityComponent>())
        {
            ref var integrity = ref neighbor.Get<StructuralIntegrityComponent>();
            integrity.NeedsRecalculation = true;
        }
    }
}