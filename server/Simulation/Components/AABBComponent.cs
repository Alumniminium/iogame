using System.Collections.Generic;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct AABBComponent(NTT EntityId, RectangleF aabb)
{
    public readonly NTT EntityId = EntityId;
    public RectangleF AABB = aabb;
    public readonly List<NTT> PotentialCollisions = new();


}