using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public static class LifetimeSystem
    {
        static LifetimeSystem() => PerformanceMetrics.RegisterSystem(nameof(LifetimeSystem));
        public static void Update(float deltaTime, Entity entity)
        {
            if (entity is Bullet bullet)
            {
                bullet.LifeTimeSeconds -= deltaTime;

                if (bullet.LifeTimeSeconds <= 0)
                    EntityManager.RemoveEntity(bullet);
            }
        }
    }
}