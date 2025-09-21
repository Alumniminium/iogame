using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct EngineComponent(NTT EntityId, float maxThrustNewtons)
{
    public readonly NTT EntityId = EntityId;
    public float PowerUse = maxThrustNewtons * 0.01f; // Power scales with thrust
    public float Throttle = 0;
    public float MaxThrustNewtons = maxThrustNewtons; // Thrust in Newtons
    public bool RCS = true;
    public float Rotation = 0;
    public long ChangedTick = NttWorld.Tick;
}