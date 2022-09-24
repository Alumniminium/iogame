using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public sealed class WeaponSystem : PixelSystem<PhysicsComponent, WeaponComponent, EnergyComponent>
    {
        public WeaponSystem() : base("Weapon System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref WeaponComponent wep, ref EnergyComponent nrg)
        {
            wep.LastShot += TimeSpan.FromSeconds(deltaTime);

            if (!wep.Fire)
                return;

            if (wep.LastShot < wep.Frequency)
                return;

            wep.LastShot = TimeSpan.Zero;

            var powerReq = wep.PowerUse * wep.BulletCount * wep.BulletSpeed / 100;

            if (nrg.AvailableCharge < powerReq)
                return;

            nrg.DiscargeRateAcc += powerReq;

            wep.Fire = false;

            var direction = phy.Forward.ToRadians() + wep.Direction.ToRadians();
            var bulletCount = wep.BulletCount;
            var d = bulletCount > 1 ? MathF.PI * 2 / bulletCount : 0;
            direction -= bulletCount > 1 ? d * bulletCount / 2 : 0;

            for (var x = 0; x < bulletCount; x++)
            {
                var dx = MathF.Cos(direction + (d * x));
                var dy = MathF.Sin(direction + (d * x));

                var bulletX = -dx + phy.Position.X;
                var bulletY = -dy + phy.Position.Y;
                var bulletPos = new Vector2(bulletX, bulletY);

                var dist = phy.Position - bulletPos;
                var penDepth = phy.Size - wep.BulletSize - dist.Length();
                var penRes = Vector2.Normalize(dist) * penDepth;
                bulletPos += penRes * 1.25f;

                if (bulletPos.X + (wep.BulletSize / 2) > Game.MapSize.X || bulletPos.X - (wep.BulletSize / 2) < 0 || bulletPos.Y + (wep.BulletSize / 2) > Game.MapSize.Y || bulletPos.Y - (wep.BulletSize / 2) < 0)
                    continue;
                var velocity = new Vector2(dx, dy) * wep.BulletSpeed;
                SpawnManager.SpawnBullets(in ntt, ref bulletPos, ref wep, ref velocity, Convert.ToUInt32("80ED99", 16));
            }
        }
    }
}