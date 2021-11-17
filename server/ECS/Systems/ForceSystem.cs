using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class ForceSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent>
    {
        public ForceSystem()
        {
            Name = "Move System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float deltaTime, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref readonly var phy = ref entity.Get<PhysicsComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                
                vel.Force *= 1f - phy.Drag;

                // if (vel.Force.Magnitude() < 5)
                //     vel.Force = Vector2.Zero;

                pos.LastPosition = pos.Position;
                pos.Position += vel.Force * deltaTime;
                pos.Position = Vector2.Clamp(pos.Position, Vector2.Zero, new Vector2(Game.MAP_WIDTH, Game.MAP_HEIGHT));
            }
        }
    }
}