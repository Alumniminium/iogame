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

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref WeaponComponent wep, ref ShapeComponent shp)
        {
            if (!wep.Fire)
                return;
            if (wep.LastShot + 5 > Game.CurrentTick)
                return;

            wep.Fire = false;
            wep.LastShot = Game.CurrentTick;

            var direction = phy.Forward.ToRadians() + wep.Direction.ToRadians();
            var bulletCount = wep.BulletCount;
            var d = bulletCount > 1 ? MathF.PI * 2 / bulletCount : 0;
            direction -= bulletCount > 1 ? d * bulletCount / 2 : 0;
            for (var x = 0; x < bulletCount; x++)
            {
                var dx = MathF.Cos(direction + d * x);
                var dy = MathF.Sin(direction + d * x);

                var bulletX = -dx + phy.Position.X;
                var bulletY = -dy + phy.Position.Y;
                var bulletPos = new Vector2(bulletX, bulletY);

                var bulletSize = 10;
                var bulletSpeed = 250;

                var dist = phy.Position - bulletPos;
                var penDepth = shp.Radius - bulletSize - dist.Length();
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