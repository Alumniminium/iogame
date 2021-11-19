using System.IO.Pipelines;
using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class BoidSystem : PixelSystem<PositionComponent, VelocityComponent, InputComponent, BoidComponent>
    {
        public BoidSystem()
        {
            Name = "BoidSystem System";
            PerformanceMetrics.RegisterSystem(Name);
        }
        public float TimePassed = 0f;
        public Vector2 targetVector = Vector2.Zero;
        public override void Update(float dt, List<Entity> Entities)
        {
            TimePassed += dt;
            if (TimePassed > 7)
            {
                TimePassed = 0f;
                targetVector.X = Random.Shared.NextSingle() * Game.MAP_WIDTH;
                targetVector.Y = Random.Shared.NextSingle() * Game.MAP_HEIGHT;
            }
            for (int i = 0; i < Entities.Count; i++)
            {

                var entity = Entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref readonly var pos = ref entity.Get<PositionComponent>();
                var shp = (Boid)World.GetAttachedShapeEntity(ref entity);

                shp.Viewport.Update();

                var flockCenter = Vector2.Zero;
                var avgVelocity = Vector2.Zero;
                var closestDistance = float.MaxValue;
                ShapeEntity closestEntity = null;

                var total = 0;
                foreach (var kvp in shp.Viewport.Entities)
                {
                    if (kvp.Key == shp.EntityId)
                        continue;

                    if (!kvp.Value.Entity.Has<BoidComponent>())
                        continue;

                    ref readonly var otherPos = ref kvp.Value.Entity.Get<PositionComponent>();
                    ref readonly var otherVel = ref kvp.Value.Entity.Get<VelocityComponent>();

                    var dist = Vector2.Distance(pos.Position, otherPos.Position);
                    if (dist < closestDistance)
                    {
                        if (closestDistance < shp.VIEW_DISTANCE / 2 && closestDistance > dist)
                        {
                            closestDistance = dist;
                            closestEntity = kvp.Value;
                        }
                    }
                    flockCenter += otherPos.Position;
                    avgVelocity += otherVel.Velocity;
                    total++;

                }
                if (i % 40 == 0)
                    inp.MovementAxis += (targetVector - pos.Position).Unit() * 0.75f;

                if (closestEntity != null)
                {
                    ref readonly var closestPos = ref closestEntity.PositionComponent;
                    var avoidanceVector = pos.Position - closestPos.Position;
                    inp.MovementAxis -= Vector2.Normalize(avoidanceVector) * avoidanceVector.Magnitude();
                }

                if (total > 0)
                {
                    flockCenter /= total;
                    avgVelocity /= total;
                    inp.MovementAxis += avgVelocity.Unit() * 0.5f;
                    inp.MovementAxis += (flockCenter - pos.Position).Unit() * 0.25f;
                }

                inp.MovementAxis = inp.MovementAxis.ClampMagnitude(1);
            }
        }
    }
}