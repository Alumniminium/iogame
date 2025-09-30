using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Manages entities with limited lifetimes (e.g., bullets, drops, temporary effects).
/// Tags entities for death when their lifetime expires.
/// </summary>
public sealed class LifetimeSystem : NttSystem<LifeTimeComponent>
{
    public LifetimeSystem() : base("Lifetime System", threads: 1) { }

    public override void Update(in NTT ntt, ref LifeTimeComponent c1)
    {
        c1.LifeTimeSeconds -= DeltaTime;

        if (c1.LifeTimeSeconds > 0)
            return;

        var dtc = new DeathTagComponent(default);
        ntt.Set(ref dtc);
    }
}