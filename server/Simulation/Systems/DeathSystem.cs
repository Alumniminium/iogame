using server.ECS;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

/// <summary>
/// Handles entity death processing including kill credit, respawn tagging for players, and entity cleanup.
/// Broadcasts death messages and manages physics body destruction.
/// </summary>
public sealed class DeathSystem : NttSystem<DeathTagComponent>
{
    public DeathSystem() : base("Death System", threads: 1) { }

    public override void Update(in NTT ntt, ref DeathTagComponent dtc)
    {
        if (dtc.Killer != default)
        {
            if (!NttWorld.EntityExists(dtc.Killer))
                return;
            var killer = NttWorld.GetEntity(dtc.Killer);
            if (killer.Has<NameTagComponent>() && ntt.Has<NameTagComponent>())
            {
                ref readonly var killerNameTag = ref killer.Get<NameTagComponent>();
                ref readonly var killedNameTag = ref ntt.Get<NameTagComponent>();
                Game.Broadcast(ChatPacket.Create(ntt, $"{killedNameTag.Name} was killed by {killerNameTag.Name}"));
            }
        }
        if (ntt.Has<NetworkComponent>())
        {
            ref var eng = ref ntt.Get<EngineComponent>();
            eng.Throttle /= 2;

            var rtc = new RespawnTagComponent(1000, 5);
            ntt.Set(ref rtc);
            ntt.Remove<DeathTagComponent>();
            ntt.Remove<InputComponent>();
        }

        if (ntt.Has<PhysicsComponent>())
        {
            var phy = ntt.Get<PhysicsComponent>();
            PhysicsWorld.DestroyBody(phy.BodyId);
        }

        NttWorld.Destroy(ntt);

    }
}