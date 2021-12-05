using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class BoidSystem : PixelSystem<PositionComponent, VelocityComponent, InputComponent, BoidComponent, ViewportComponent>
    {
        public BoidSystem() : base("BoidSystem System", Environment.ProcessorCount){}
        public Vector2 targetVector = new(Game.MAP_WIDTH / 2, Game.MAP_HEIGHT / 2);
        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref readonly var boi = ref entity.Get<BoidComponent>();
                ref readonly var vwp = ref entity.Get<ViewportComponent>();

                inp.MovementAxis = Vector2.Zero;
                var flockCenter = Vector2.Zero;
                var avgVelocity = Vector2.Zero;
                var avoidanceVector = Vector2.Zero;

                var total = 0;
                var totalClose = 0f;

                if (vwp.EntitiesVisible != null)
                {
                    for (int k = 0; k < vwp.EntitiesVisible.Length; k++)
                    {
                        ref var other = ref PixelWorld.GetEntity(vwp.EntitiesVisible[k].EntityId);

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

                        if (otherBoi.Flock == boi.Flock)
                        {
                            flockCenter += otherPos.Position;
                            avgVelocity += otherVel.Velocity;
                            total++;
                        }
                    }
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
                inp.MovementAxis += targetVector - pos.Position;
                inp.MovementAxis = Vector2.Normalize(inp.MovementAxis);
            }
        }
    }
}