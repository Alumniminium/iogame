using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct EngineComponent(NTT EntityId, ushort maxPropulsion)
{
    public readonly NTT EntityId = EntityId;
    public float PowerUse = maxPropulsion * 2;
    public float Throttle = 0;
    public ushort MaxPropulsion = maxPropulsion;
    public bool RCS = true;
    public float Rotation = 0;
    public long ChangedTick = NttWorld.Tick;
}