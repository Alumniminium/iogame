using server.ECS;

namespace server.Simulation.Components.Replication
{
    [Component]
    public readonly struct HealthReplicationComponent
    {
        public readonly uint ChangedTick;
        public readonly float ClientHealth;
        public readonly int ClientMaxHealth;
        public readonly float ClientPassiveHealPerSec;

        public HealthReplicationComponent(in HealthComponent hlt)
        {
            ChangedTick = Game.CurrentTick;
            ClientHealth = hlt.Health;
            ClientMaxHealth=hlt.MaxHealth;
            ClientPassiveHealPerSec = hlt.PassiveHealPerSec;
        }
    }
}