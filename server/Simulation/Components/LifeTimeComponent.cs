using System;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct LifeTimeComponent(int entityId, TimeSpan timespan)
{
    public readonly int EntityId = entityId;
    public float LifeTimeSeconds = (float)timespan.TotalSeconds;

    public override int GetHashCode() => EntityId;
}