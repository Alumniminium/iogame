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
        public Vector2 targetVector2 = Vector2.Zero;
        public Vector2 targetVector3 = Vector2.Zero;
        public override void Update(float dt, List<Entity> Entities)
        {
            TimePassed += dt;
            if (TimePassed > 5)
            {
                TimePassed = 0f;
                targetVector.X = Random.Shared.NextSingle() * Game.MAP_WIDTH;
                targetVector.Y = Random.Shared.NextSingle() * Game.MAP_HEIGHT;
                targetVector2.X = Random.Shared.NextSingle() * Game.MAP_WIDTH;
                targetVector2.Y = Random.Shared.NextSingle() * Game.MAP_HEIGHT;
                targetVector3.X = Random.Shared.NextSingle() * Game.MAP_WIDTH;
                targetVector3.Y = Random.Shared.NextSingle() * Game.MAP_HEIGHT;
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
                if (i % 3 == 0)
                    inp.MovementAxis += (targetVector - pos.Position).Unit();
                else if (i % 2 == 0)
                    inp.MovementAxis += (targetVector2 - pos.Position).Unit();
                else
                    inp.MovementAxis += (targetVector3 - pos.Position).Unit();

                if (closestEntity != null)
                {
                    ref readonly var closestPos = ref closestEntity.PositionComponent;
                    var avoidanceVector = pos.Position - closestPos.Position;
                    inp.MovementAxis += Vector2.Normalize(avoidanceVector) * avoidanceVector.Magnitude();
                }

                if (total > 0)
                {
                    flockCenter /= total;
                    avgVelocity /= total;
                    inp.MovementAxis += avgVelocity.Unit();
                    inp.MovementAxis += (flockCenter - pos.Position).Unit();
                }

                inp.MovementAxis = inp.MovementAxis.ClampMagnitude(1);
            }
        }
    }
}