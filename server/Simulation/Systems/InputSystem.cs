using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class InputSystem : PixelSystem<InputComponent, SpeedComponent, VelocityComponent>
    {
        public InputSystem() : base("Input System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref readonly var spd = ref entity.Get<SpeedComponent>();
                ref var inp = ref entity.Get<InputComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var shp = ref entity.Get<ShapeComponent>();

                vel.Acceleration = inp.MovementAxis * spd.Speed * dt;

                if (!inp.Fire)
                    continue;
                if (inp.LastShot + 10 > Game.CurrentTick)
                    continue;

                inp.LastShot = Game.CurrentTick;

                var direction = (float)Math.Atan2(inp.MousePositionWorld.Y, inp.MousePositionWorld.X);
                var dx = (float)Math.Cos(direction);
                var dy = (float)Math.Sin(direction);

                var bulletX = -dx + pos.Position.X;
                var bulletY = -dy + pos.Position.Y;
                var bulletPos = new Vector2(bulletX, bulletY);
                var bulletSize = 10;
                var bulletSpeed = 125;

                var dist = pos.Position - bulletPos;
                var penDepth = shp.Radius + bulletSize - dist.Length();
                var penRes = Vector2.Normalize(dist) * penDepth * 1.125f;
                bulletPos += penRes;
                var velocity = new Vector2(dx, dy) * bulletSpeed;
                SpawnManager.SpawnBullets(ref entity, ref bulletPos, ref velocity);
            }
        }
    }
}