using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Collision, NetworkSync = false)]
public readonly struct CollisionComponent(NTT a)
{
    public readonly NTT A = a;
    public readonly List<(NTT, Vector2, float)> Collisions = new(5);
}