using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct EngineComponent(int entityId, ushort maxPropulsion)
{
    public readonly int EntityId = entityId;
    public float PowerUse = maxPropulsion * 2;
    public float Throttle = 0;
    public ushort MaxPropulsion = maxPropulsion;
    public bool RCS = true;
    public float Rotation = 0;
    public uint ChangedTick = Game.CurrentTick;

    public override int GetHashCode() => EntityId;
}