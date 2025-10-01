using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Health, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HealthComponent(float health, float maxHealth)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public float Health = health;
    public float MaxHealth = maxHealth;
}