using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component]
public readonly struct CollisionComponent(PixelEntity a)
{
    public readonly EntityType EntityTypes = a.Type;
    public readonly PixelEntity A = a;
    public readonly List<(PixelEntity, Vector2, float)> Collisions = new List<(PixelEntity, Vector2, float)>(5);

    public override int GetHashCode() => A.Id;
}