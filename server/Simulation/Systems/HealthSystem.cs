using server.ECS;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();

                hlt.Health += hlt.PassiveHealPerSec * dt;

                if(entity.Has<DamageComponent>())
                {
                    ref readonly var dmg = ref entity.Get<DamageComponent>();
                    hlt.Health -= dmg.Damage;
                    entity.Remove<DamageComponent>();
                }

                if (hlt.Health > hlt.MaxHealth)
                    hlt.Health = hlt.MaxHealth;

                if(hlt.LastHealth == hlt.Health)
                    continue;

                if (hlt.Health <= 0)
                    PixelWorld.Destroy(in entity);
                else
                    Game.Broadcast(StatusPacket.Create(entity.EntityId, (uint)hlt.Health, StatusType.Health));

                hlt.LastHealth = hlt.Health;
            }
        }
    }
}