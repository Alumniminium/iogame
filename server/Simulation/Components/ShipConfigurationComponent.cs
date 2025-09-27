using System.Collections.Generic;
using server.ECS;
using server.Simulation.Net;

namespace server.Simulation.Components;

[Component]
public struct ShipConfigurationComponent(NTT entity, List<ShipPart> parts)
{
    public NTT Entity = entity;
    public List<ShipPart> Parts = parts;
}