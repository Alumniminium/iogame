using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class BoidSystem : PixelSystem<PhysicsComponent, InputComponent, BoidComponent, ViewportComponent>
    {
        public BoidSystem() : base("BoidSystem System", threads: Environment.ProcessorCount){}
        private Vector2 _targetVector = Game.MapSize / 2;

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            if(Random.Shared.Next(0,1000) == 1)
                _targetVector = new Vector2(Random.Shared.Next(0, (int)Game.MapSize.X), Random.Shared.Next(0, (int)Game.MapSize.Y));

        for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref readonly var boi = ref entity.Get<BoidComponent>();
                ref readonly var vwp = ref entity.Get<ViewportComponent>();
                
                ref var inp = ref entity.Get<InputComponent>();
                ref var phy = ref entity.Get<PhysicsComponent>();

                inp.MovementAxis = Vector2.Zero;
                var flockCenter = Vector2.Zero;
                var avgVelocity = Vector2.Zero;
                var avoidanceVector = Vector2.Zero;

                var total = 0;
                var totalClose = 0f;

                for (var k = 0; k < vwp.EntitiesVisible.Count; k++)
                {
                    ref var other = ref PixelWorld.GetEntity(vwp.EntitiesVisible[k].Entity.Id);

                    ref var otherPhy = ref other.Get<PhysicsComponent>();

                    var dist = Vector2.Distance(phy.Position, otherPhy.Position);

                    if (dist < vwp.ViewDistance / 2)
                    {
                        var d = phy.Position-otherPhy.Position;
                        avoidanceVector += d;
                        totalClose++;
                    }

                    if (!other.Has<BoidComponent>())
                        continue;

                    ref readonly var otherBoi = ref other.Get<BoidComponent>();
                    
                    if (otherBoi.Flock != boi.Flock) 
                        continue;
                    
                    flockCenter += otherPhy.Position;
                    avgVelocity += otherPhy.Velocity;
                    total++;
                }

                if (total > 0 && flockCenter != Vector2.Zero)
                {
                    flockCenter /= total;
                    inp.MovementAxis += (flockCenter - phy.Position) * 0.01f;
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
                inp.MovementAxis += (_targetVector - phy.Position) * 0.2f;
                inp.MovementAxis = Vector2.Normalize(inp.MovementAxis);
            }
        }
    }
}