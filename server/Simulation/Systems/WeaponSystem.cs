using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class WeaponSystem : PixelSystem<PhysicsComponent, WeaponComponent, ShapeComponent>
    {
        public WeaponSystem() : base("Weapon System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent c1, ref WeaponComponent c2, ref ShapeComponent c3)
        {
            if (!c2.Fire)
                return;
            if (c2.LastShot + 5 > Game.CurrentTick)
                return;

            c2.Fire = false;
            c2.LastShot = Game.CurrentTick;

            var direction = c1.Forward.ToRadians() + c2.Direction.ToRadians();
            var bulletCount = c2.BulletCount;
            var d = bulletCount > 1 ? MathF.PI * 2 / bulletCount : 0;
            direction -= bulletCount > 1 ? d * bulletCount / 2 : 0;
            for (var x = 0; x < bulletCount; x++)
            {
                var dx = MathF.Cos(direction + d * x);
                var dy = MathF.Sin(direction + d * x);

                var bulletX = -dx + c1.Position.X;
                var bulletY = -dy + c1.Position.Y;
                var bulletPos = new Vector2(bulletX, bulletY);

                var bulletSize = 10;
                var bulletSpeed = 250;

                var dist = c1.Position - bulletPos;
                var penDepth = c3.Radius - bulletSize - dist.Length();
                var penRes = Vector2.Normalize(dist) * penDepth * 1.25f;
                bulletPos += penRes;

                if (bulletPos.X + bulletSize / 2 > Game.MapSize.X || bulletPos.X - bulletSize / 2 < 0 || bulletPos.Y + bulletSize / 2 > Game.MapSize.Y || bulletPos.Y - bulletSize / 2 < 0)
                    continue;

                var velocity = new Vector2(dx, dy) * bulletSpeed;
                SpawnManager.SpawnBullets(in ntt, ref bulletPos, ref velocity);
            }
        }
    }
}