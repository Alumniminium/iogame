using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public static class RotationSystem
    {
        static RotationSystem() => PerformanceMetrics.RegisterSystem(nameof(RotationSystem));
        public static unsafe void Update(float deltaTime, Entity entity)
        {
            var (vel, spin, _) = entity.VelocityComponent;
            var radians = Math.Atan2(vel.X, vel.Y);
            var rot = (float)(180 * radians / Math.PI);

            rot += spin * deltaTime;

            if (rot > 360)
                rot -= 360;
            if (rot < 0)
                rot += 360;

            entity.PositionComponent.Rotation = rot;
        }

    }
}