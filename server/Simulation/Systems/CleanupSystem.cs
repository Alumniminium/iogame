using server.ECS;
using server.Helpers;
using server.Serialization;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class CleanupSystem : NttSystem<DeathTagComponent>
{
    public CleanupSystem() : base("Cleanup System", threads: 1) { }

    public override void Update(in NTT ntt, ref DeathTagComponent dtc)
    {
        FConsole.WriteLine($"[CleanupSystem] Broadcasting DeathTag for entity {ntt.Id}");
        Game.Broadcast(ComponentSerializer.Serialize(ntt, ref dtc));
    }
}