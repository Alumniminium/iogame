using iogame.Simulation.Entities;

namespace iogame.Simulation.Systems
{
    public static class HealthSystem
    {
        public static void Update(float deltaTime, Entity entity)
        {
            if (entity.HealthComponent.Health <= 0)
            {
                Game.RemoveEntity(entity);
                return;
            }

            if (entity.HealthComponent.Health < entity.HealthComponent.MaxHealth)
            {
                var healthAdd = 1 * deltaTime;
                if (entity.HealthComponent.Health + healthAdd > entity.HealthComponent.MaxHealth)
                    entity.HealthComponent.Health = entity.HealthComponent.MaxHealth;
                else
                    entity.HealthComponent.Health += healthAdd;
            }
        }
    }
}