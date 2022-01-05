using server.ECS;
using server.Simulation.Components;
using server.Simulation.Components.Replication;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt)
        {
            var lastHealth = hlt.Health;
            hlt.Health += hlt.PassiveHealPerSec * deltaTime;

            if (hlt.Health > hlt.MaxHealth)
                hlt.Health = hlt.MaxHealth;
           
            if(lastHealth == hlt.Health)
                return;

            var hltRepl = new HealthReplicationComponent(in hlt);
            ntt.Replace(ref hltRepl);
        }
    }
}