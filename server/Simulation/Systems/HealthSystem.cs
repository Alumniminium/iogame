using iogame.ECS;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", Environment.ProcessorCount) { }

        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();

                if (hlt.Health == hlt.MaxHealth)
                    continue;

                hlt.Health += hlt.HealthRegenFactor * dt;

                if (hlt.Health > hlt.MaxHealth)
                    hlt.Health = hlt.MaxHealth;

                if (hlt.Health <= 0)
                {
                    hlt.Health = 0;
                    PixelWorld.Destroy(entity.EntityId);
                    Game.Broadcast(StatusPacket.CreateDespawn(entity.EntityId));
                }
                else
                {
                    // Game.Broadcast(StatusPacket.Create(entity.EntityId, (uint)hlt.Health, StatusType.Health));
                }
            }
        }
    }
}