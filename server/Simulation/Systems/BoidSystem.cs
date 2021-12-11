using System;
using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class BoidSystem : PixelSystem<PositionComponent, InputComponent, BoidComponent, ViewportComponent>
    {
        public BoidSystem() : base("BoidSystem System", Environment.ProcessorCount){}
        private readonly Vector2 _targetVector = new(Game.MapWidth / 2, Game.MapHeight / 2);

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
                ref readonly var boi = ref entity.Get<BoidComponent>();
                ref readonly var vwp = ref entity.Get<ViewportComponent>();
                
                ref var inp = ref entity.Get<InputComponent>();
                ref var pos = ref entity.Get<PositionComponent>();

                inp.MovementAxis = Vector2.Zero;
                var flockCenter = Vector2.Zero;
                var avgVelocity = Vector2.Zero;
                var avoidanceVector = Vector2.Zero;

                var total = 0;
                var totalClose = 0f;

                for (var k = 0; k < vwp.EntitiesVisible.Length; k++)
                {
                    if(!PixelWorld.EntityExists(vwp.EntitiesVisible[k].Entity.EntityId))
                        continue; 
                        
                    ref var other = ref PixelWorld.GetEntity(vwp.EntitiesVisible[k].Entity.EntityId);

                    if (entity.EntityId == other.EntityId)
                        continue;

                    if (!other.Has<PositionComponent, VelocityComponent>())
                        continue;

                    ref var otherPos = ref other.Get<PositionComponent>();
                    ref var otherVel = ref other.Get<VelocityComponent>();

                    var dist = Vector2.Distance(pos.Position, otherPos.Position);

                    if (dist < vwp.ViewDistance)
                    {
                        var d = pos.Position - otherPos.Position;
                        avoidanceVector += Vector2.Normalize(d) * d.Length();
                        totalClose++;
                    }

                    if (!other.Has<BoidComponent>())
                        continue;

                    ref readonly var otherBoi = ref other.Get<BoidComponent>();
                    
                    if (otherBoi.Flock != boi.Flock) 
                        continue;
                    
                    flockCenter += otherPos.Position;
                    avgVelocity += otherVel.Velocity;
                    total++;
                }

                if (total > 0 && flockCenter != Vector2.Zero)
                {
                    flockCenter /= total;
                    inp.MovementAxis += Vector2.Normalize(flockCenter - pos.Position);
                }
                if (total > 0 && avgVelocity != Vector2.Zero)
                {
                    avgVelocity /= total;
                    inp.MovementAxis += Vector2.Normalize(avgVelocity);
                }
                if (totalClose > 0 && avoidanceVector != Vector2.Zero)
                {
                    avoidanceVector /= totalClose;
                    inp.MovementAxis += Vector2.Normalize(avoidanceVector);
                }
                inp.MovementAxis += _targetVector - pos.Position;
                inp.MovementAxis = Vector2.Normalize(inp.MovementAxis);
            }
        }
    }
}