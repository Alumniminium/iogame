using server.ECS;
using server.Simulation.Components;
using server.Simulation.Components.Replication;

namespace server.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", threads: 12) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt)
        {
            var lastHealth = hlt.Health;
            hlt.Health += hlt.PassiveHealPerSec * deltaTime;

            if (hlt.Health > hlt.MaxHealth)
                hlt.Health = hlt.MaxHealth;

            if (hlt.Health <= 0)
                PixelWorld.Destroy(in ntt);
            
            if(lastHealth == hlt.Health)
                return;

            var hltRepl = new HealthReplicationComponent(in hlt);
            ntt.Set(ref hltRepl);
        }
    }
}