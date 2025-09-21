using System.Collections.Generic;
using System.Numerics;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct CollisionComponent(NTT a)
{
    public readonly NTT A = a;
    public readonly List<(NTT, Vector2, float)> Collisions = new(5);
}