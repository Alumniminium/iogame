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
            if (TimePassed > 15)
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

                var total = 0;
                var totalClose = 0f;
                foreach (var kvp in shp.Viewport.Entities)
                {
                    ref readonly var otherPos = ref kvp.Value.Entity.Get<PositionComponent>();
                    ref readonly var otherVel = ref kvp.Value.Entity.Get<VelocityComponent>();

                    var dist = Vector2.Distance(pos.Position, otherPos.Position);
                    if (dist < shp.VIEW_DISTANCE / 1.5)
                    {
                        ref readonly var closestPos = ref kvp.Value.PositionComponent;
                        var avoidanceVector = pos.Position - closestPos.Position;
                        inp.MovementAxis += Vector2.Normalize(avoidanceVector) / avoidanceVector.Magnitude();
                        totalClose++;
                    }

                    if (kvp.Value.Entity.Has<BoidComponent>())
                    {
                        ref readonly var otherBoi = ref kvp.Value.Entity.Get<BoidComponent>();

                        if (otherBoi.Flock != boi.Flock)
                            continue;

                        flockCenter += otherPos.Position;
                        avgVelocity += otherVel.Velocity;
                        total++;
                    }
                }
                if (totalClose > 0)
                    inp.MovementAxis /= totalClose;
                switch (boi.Flock)
                {
                    case 0:
                        inp.MovementAxis += (targetVector - pos.Position).Unit() * 0.25f;
                        break;
                    case 1:
                        inp.MovementAxis += (targetVector2 - pos.Position).Unit() * 0.25f;
                        break;
                    case 2:
                        inp.MovementAxis += (targetVector3 - pos.Position).Unit() * 0.25f;
                        break;
                }

                if (total > 0)
                {
                    flockCenter /= total;
                    avgVelocity /= total;
                    inp.MovementAxis += avgVelocity.Unit() * 0.25f;
                    inp.MovementAxis += (flockCenter - pos.Position).Unit() * 0.25f;
                }

                inp.MovementAxis = inp.MovementAxis.ClampMagnitude(1);
            }
        }
    }
}