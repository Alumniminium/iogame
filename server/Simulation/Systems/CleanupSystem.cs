using server.ECS;
using server.Helpers;
using server.Serialization;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Broadcasts death tag components to all clients before entities are destroyed.
/// Ensures clients are notified of entity deaths for proper cleanup and visual effects.
/// </summary>
public sealed class CleanupSystem : NttSystem<DeathTagComponent>
{
    public CleanupSystem() : base("Cleanup System", threads: 1) { }

    public override void Update(in NTT ntt, ref DeathTagComponent dtc)
    {
        Game.Broadcast(ComponentSerializer.Serialize(ntt, ref dtc));
    }
}