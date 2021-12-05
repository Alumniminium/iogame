using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class InputSystem : PixelSystem<InputComponent, SpeedComponent, VelocityComponent>
    {
        public InputSystem() : base("Input System", Environment.ProcessorCount) { }
        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref readonly var spd = ref entity.Get<SpeedComponent>();
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
                var pen_depth = shp.Radius + bulletSize - dist.Length();
                var pen_res = Vector2.Normalize(dist) * pen_depth * 1.125f;
                bulletPos += pen_res;

                var bullet = SpawnManager.SpawnBullets(bulletPos, new Vector2(dx, dy) * bulletSpeed);
                bullet.Owner = entity;
            }
        }
    }
}