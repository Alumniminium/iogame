using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class CollisionCleanupSystem : NttSystem
{
    public CollisionCleanupSystem() : base("Collision Cleanup System", threads: 1) { }

    protected override bool MatchesFilter(in NTT ntt) => ntt.Has<CollisionComponent>() && base.MatchesFilter(in ntt);

    protected override void Update(int start, int amount)
    {
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
            span[i].Remove<CollisionComponent>();
    }
}
