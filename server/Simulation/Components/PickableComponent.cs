using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.PickableTag, NetworkSync = true)]
public struct PickableTagComponent()
{
    public long ChangedTick = NttWorld.Tick;
}