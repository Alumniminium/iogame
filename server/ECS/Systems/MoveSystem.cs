using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{

    public class MoveSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent>
    {
        public MoveSystem()
        { 
            Name  = "Move System";
        }
        
        public override void Update(float deltaTime, List<Entity> Entities)
        {
            for(int i =0; i< Entities.Count; i++)
            {
                var entity = Entities[i];
                var shapeEntity = World.GetAttachedShapeEntity(entity);
                    
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref var phy = ref entity.Get<PhysicsComponent>();

                vel.Movement *= 1f - (phy.Drag * deltaTime);

                if (vel.Movement.Magnitude() < 5)
                    vel.Movement = Vector2.Zero;

                pos.LastPosition = pos.Position;
                pos.Position += vel.Movement * deltaTime;
                pos.Position = Vector2.Clamp(pos.Position, Vector2.Zero, new Vector2(Game.MAP_WIDTH, Game.MAP_HEIGHT));
            }
        }
    }
}