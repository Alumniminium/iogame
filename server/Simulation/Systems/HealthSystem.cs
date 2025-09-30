using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Manages passive health regeneration for entities with health regen components.
/// Regenerates health over time up to the maximum health value.
/// </summary>
public sealed class HealthSystem : NttSystem<HealthComponent, HealthRegenComponent>
{
    public HealthSystem() : base("Health System", threads: 1) { }

        public override void Update(in NTT ntt, ref HealthComponent c1, ref HealthRegenComponent reg)
        {
            if (c1.Health == c1.MaxHealth)
                return;
            if (ntt.Has<RespawnTagComponent>())
                return;

            var lastHealth = c1.Health;
            c1.Health += reg.PassiveHealPerSec * DeltaTime;

            if (c1.Health > c1.MaxHealth)
                c1.Health = c1.MaxHealth;

            if (lastHealth != c1.Health)
                c1.ChangedTick = NttWorld.Tick;
    }
}