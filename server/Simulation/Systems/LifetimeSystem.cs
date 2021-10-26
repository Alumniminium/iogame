using iogame.Simulation.Entities;

namespace iogame.Simulation.Systems
{
    public static class LifetimeSystem
    {
        public static void Update(float deltaTime, Entity entity)
        {
            if (entity is Bullet bullet)
            {
                bullet.LifeTimeSeconds -= deltaTime;

                if (bullet.LifeTimeSeconds <= 0)
                    Game.RemoveEntity(bullet);
            }
        }
    }
}