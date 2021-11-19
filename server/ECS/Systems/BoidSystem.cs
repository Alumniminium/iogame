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
        public int counter = 0;
        public Vector2 targetVector = new(Game.MAP_WIDTH / 2, Game.MAP_HEIGHT / 2);
        public Vector2 targetVector2 = new(Game.MAP_WIDTH / 2, Game.MAP_HEIGHT / 2);
        public Vector2 targetVector3 = new(Game.MAP_WIDTH / 2, Game.MAP_HEIGHT / 2);
        public override void Update(float dt, List<Entity> Entities)
        {
            TimePassed += dt;
            if (TimePassed > 10)
            {
                TimePassed = 0f;
                switch (counter)
                {
                    case 0:
                        targetVector.X = Random.Shared.Next(50, Game.MAP_WIDTH - 50);
                        targetVector.Y = Random.Shared.Next(50, Game.MAP_HEIGHT - 50);
                        break;
                    case 1:
                        targetVector2.X = Random.Shared.Next(50, Game.MAP_WIDTH - 50);
                        targetVector2.Y = Random.Shared.Next(50, Game.MAP_HEIGHT - 50);
                        break;
                    case 2:
                        targetVector3.X = Random.Shared.Next(50, Game.MAP_WIDTH - 50);
                        targetVector3.Y = Random.Shared.Next(50, Game.MAP_HEIGHT - 50);
                        break;
                }
                counter++;
                if (counter > 2)
                    counter = 0;
            }
            for (int i = 0; i < Entities.Count; i++)
            {

                var entity = Entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref readonly var pos = ref entity.Get<PositionComponent>();
                ref readonly var boi = ref entity.Get<BoidComponent>();
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
                    ref readonly var otherBoi = ref kvp.Value.Entity.Get<BoidComponent>();


                    if (otherBoi.Flock == boi.Flock)
                    {
                        var dist = Vector2.Distance(pos.Position, otherPos.Position);
                        if (dist < closestDistance)
                        {
                            if (dist < shp.VIEW_DISTANCE / 2 && closestDistance > dist)
                            {
                                closestDistance = dist;
                                closestEntity = kvp.Value;
                            }
                        }
                    }

                    if (otherBoi.Flock != boi.Flock)
                        continue;

                    flockCenter += otherPos.Position;
                    avgVelocity += otherVel.Velocity;
                    total++;

                }
                switch (boi.Flock)
                {
                    case 0:
                        inp.MovementAxis += (targetVector - pos.Position).Unit();
                        break;
                    case 1:
                        inp.MovementAxis += (targetVector2 - pos.Position).Unit();
                        break;
                    case 2:
                        inp.MovementAxis += (targetVector3 - pos.Position).Unit();
                        break;
                }
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