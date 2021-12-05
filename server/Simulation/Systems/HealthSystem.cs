using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", Environment.ProcessorCount) { }

        public override void Update(float dt, RefList<PixelEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                ref readonly var entity = ref entities[i];
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