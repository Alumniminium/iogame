using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public sealed class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent dtc)
        {
            if (dtc.KillerId != 0)
            {
                if (!PixelWorld.EntityExists(dtc.KillerId))
                    return;
                var killer = PixelWorld.GetEntity(dtc.KillerId);
                if (killer.Has<NameTagComponent>() && ntt.Has<NameTagComponent>())
                {
                    ref readonly var killerNameTag = ref killer.Get<NameTagComponent>();
                    ref readonly var killedNameTag = ref ntt.Get<NameTagComponent>();
                    Game.Broadcast(ChatPacket.Create(0, $"{killedNameTag.Name} was killed by {killerNameTag.Name}"));
                }
            }
            if (ntt.Type == EntityType.Player)
            {
                ref var eng = ref ntt.Get<EngineComponent>();
                eng.Rotation = 1;
                eng.Throttle /= 2;

                var rtc = new RespawnTagComponent(ntt.Id, 1000, 5);
                ntt.Add(ref rtc);
                ntt.Remove<DeathTagComponent>();
                ntt.Remove<InputComponent>();
                return;
            }
            Game.Grid.Remove(in ntt);
            PixelWorld.Destroy(in ntt);
        }
    }
}