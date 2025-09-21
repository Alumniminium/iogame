using System;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct LifeTimeComponent(NTT EntityId, TimeSpan timespan)
{
    public readonly NTT EntityId = EntityId;
    public float LifeTimeSeconds = (float)timespan.TotalSeconds;


}