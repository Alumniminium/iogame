using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class CleanupSystem : NttSystem<CollisionComponent>
{
    public CleanupSystem() : base("Cleanup System", threads: 1) { }
    public override void Update(in NTT ntt, ref CollisionComponent col)
    {
        ntt.Remove<CollisionComponent>();
    }
}