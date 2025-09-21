using System;
using System.Collections.Generic;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public class AsteroidStructuralIntegritySystem : NttSystem<StructuralIntegrityComponent, AsteroidNeighborComponent>
{
    public AsteroidStructuralIntegritySystem() : base("Asteroid Structural Integrity", threads: 1) { }
    private const int MAX_SUPPORT_DISTANCE = 6;

    public override void Update(in NTT ntt, ref StructuralIntegrityComponent integrity, ref AsteroidNeighborComponent neighbors)
    {
        if (!integrity.NeedsRecalculation) return;

        // BFS to find nearest anchor
        var distance = CalculateDistanceToAnchor(ntt, neighbors);

        integrity.SupportDistance = distance;
        integrity.Integrity = 1f - (distance / (float)MAX_SUPPORT_DISTANCE);
        integrity.NeedsRecalculation = false;

        // Mark for collapse if too far from support
        if (distance > MAX_SUPPORT_DISTANCE)
        {
            ntt.Set(new StructuralCollapseComponent());

            // Visual feedback before collapse
            if (ntt.Has<Box2DBodyComponent>())
            {
                ref var body = ref ntt.Get<Box2DBodyComponent>();
                // Add red tint or crack texture - lerp to red
                body.Color = LerpColor(body.Color, 0xFF0000, 0.5f);
            }
        }
    }

    private int CalculateDistanceToAnchor(NTT start, AsteroidNeighborComponent neighbors)
    {
        var visited = new HashSet<Guid>();
        var queue = new Queue<(NTT block, int distance)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();

            if (current.Id == Guid.Empty || visited.Contains(current.Id))
                continue;

            visited.Add(current.Id);

            // Check if this block is an anchor
            if (current.Has<AsteroidBlockComponent>())
            {
                var block = current.Get<AsteroidBlockComponent>();
                if (block.IsAnchor)
                    return distance; // Found nearest anchor!
            }

            // Check neighbors
            if (current.Has<AsteroidNeighborComponent>())
            {
                var currentNeighbors = current.Get<AsteroidNeighborComponent>();

                if (currentNeighbors.North.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.North, distance + 1));
                if (currentNeighbors.South.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.South, distance + 1));
                if (currentNeighbors.East.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.East, distance + 1));
                if (currentNeighbors.West.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.West, distance + 1));
            }
        }

        return int.MaxValue; // No anchor found - will collapse
    }

    private static uint LerpColor(uint fromColor, uint toColor, float t)
    {
        var r1 = (fromColor >> 16) & 0xFF;
        var g1 = (fromColor >> 8) & 0xFF;
        var b1 = fromColor & 0xFF;

        var r2 = (toColor >> 16) & 0xFF;
        var g2 = (toColor >> 8) & 0xFF;
        var b2 = toColor & 0xFF;

        var r = (uint)(r1 + (r2 - r1) * t);
        var g = (uint)(g1 + (g2 - g1) * t);
        var b = (uint)(b1 + (b2 - b1) * t);

        return 0xFF000000 | (r << 16) | (g << 8) | b;
    }
}