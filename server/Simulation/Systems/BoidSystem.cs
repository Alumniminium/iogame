using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class BoidSystem : PixelSystem<PositionComponent, InputComponent, BoidComponent, ViewportComponent>
    {
        public BoidSystem() : base("BoidSystem System", threads: Environment.ProcessorCount){}
        private Vector2 _targetVector = Game.MapSize / 2;

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            if(Random.Shared.Next(0,1000) == 1)
                _targetVector = new Vector2(Random.Shared.Next(0, (int)Game.MapSize.X), Random.Shared.Next(0, (int)Game.MapSize.Y));

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

                for (var k = 0; k < vwp.EntitiesVisible.Count; k++)
                {
                    ref var other = ref PixelWorld.GetEntity(vwp.EntitiesVisible[k].Entity.Id);

                    ref var otherPos = ref other.Get<PositionComponent>();
                    ref var otherPhy = ref other.Get<PhysicsComponent>();

                    var dist = Vector2.Distance(pos.Position, otherPos.Position);

                    if (dist < vwp.ViewDistance / 2)
                    {
                        var d = pos.Position-otherPos.Position;
                        avoidanceVector += d;
                        totalClose++;
                    }

                    if (!other.Has<BoidComponent>())
                        continue;

                    ref readonly var otherBoi = ref other.Get<BoidComponent>();
                    
                    if (otherBoi.Flock != boi.Flock) 
                        continue;
                    
                    flockCenter += otherPos.Position;
                    avgVelocity += otherPhy.Velocity;
                    total++;
                }

                if (total > 0 && flockCenter != Vector2.Zero)
                {
                    flockCenter /= total;
                    inp.MovementAxis += (flockCenter - pos.Position) * 0.01f;
                }
                if (total > 0 && avgVelocity != Vector2.Zero)
                {
                    avgVelocity /= total;
                    inp.MovementAxis += avgVelocity * 0.01f;
                }
                if (totalClose > 0 && avoidanceVector != Vector2.Zero)
                {
                    avoidanceVector /= totalClose;
                    inp.MovementAxis -= avoidanceVector * 0.1f;
                }
                inp.MovementAxis += (_targetVector - pos.Position) * 0.2f;
                inp.MovementAxis = Vector2.Normalize(inp.MovementAxis);
            }
        }
    }
}