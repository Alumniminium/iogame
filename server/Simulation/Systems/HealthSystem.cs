using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public static class HealthSystem
    {
        static HealthSystem() => PerformanceMetrics.RegisterSystem(nameof(HealthSystem));
        public static void Update(float deltaTime, Entity entity)
        {
            if (entity.HealthComponent.Health <= 0)
            {
                EntityManager.RemoveEntity(entity);
                return;
            }

            if (entity.HealthComponent.Health < entity.HealthComponent.MaxHealth)
            {
                var healthAdd = 0;//1 * deltaTime;
                if (entity.HealthComponent.Health + healthAdd > entity.HealthComponent.MaxHealth)
                    entity.HealthComponent.Health = entity.HealthComponent.MaxHealth;
                else
                    entity.HealthComponent.Health += healthAdd;
            }
        }
    }
}