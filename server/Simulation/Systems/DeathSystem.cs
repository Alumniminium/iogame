using server.ECS;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

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
            eng.Rotation = 1;
            eng.Throttle /= 2;

            var rtc = new RespawnTagComponent(ntt, 1000, 5);
            ntt.Set(ref rtc);
            ntt.Remove<DeathTagComponent>();
            ntt.Remove<InputComponent>();
            return;
        }
        Game.Grid.Remove(in ntt);
        NttWorld.Destroy(ntt);
    }
}