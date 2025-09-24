using System.Collections.Generic;
using server.ECS;
using server.Simulation.Net;

namespace server.Simulation.Components;

[Component]
public struct ShipConfigurationComponent
{
    public NTT Entity;
    public List<ShipPart> Parts;
    public sbyte CenterX;
    public sbyte CenterY;

    public ShipConfigurationComponent(NTT entity, List<ShipPart> parts, sbyte centerX, sbyte centerY)
    {
        Entity = entity;
        Parts = parts;
        CenterX = centerX;
        CenterY = centerY;
    }
}