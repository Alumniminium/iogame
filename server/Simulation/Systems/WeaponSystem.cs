using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems;

/// <summary>
/// Manages weapon firing, bullet spawning, and energy consumption.
/// Handles fire rate limiting, multi-shot patterns, and bullet positioning to prevent entity overlap.
/// </summary>
public sealed class WeaponSystem : NttSystem<PhysicsComponent, WeaponComponent, EnergyComponent>
{
    public WeaponSystem() : base("Weapon System", threads: 1) { }

    public override void Update(in NTT ntt, ref PhysicsComponent rigidBody, ref WeaponComponent wep, ref EnergyComponent nrg)
    {
        wep.LastShot += TimeSpan.FromSeconds(DeltaTime);

        if (!wep.Fire)
            return;

        if (wep.LastShot < TimeSpan.FromMilliseconds(wep.Frequency))
            return;

        wep.LastShot = TimeSpan.Zero;

        var powerReq = wep.PowerUse * wep.BulletCount * wep.BulletSpeed / 100;

        if (nrg.AvailableCharge < powerReq)
            return;

        nrg.DiscargeRateAcc += powerReq;

        wep.Fire = false;

        var forward = new Vector2(MathF.Cos(rigidBody.RotationRadians), MathF.Sin(rigidBody.RotationRadians));
        var direction = forward.ToRadians() + wep.Direction.ToRadians();
        var bulletCount = wep.BulletCount;
        var d = bulletCount > 1 ? MathF.PI * 2 / bulletCount : 0;
        direction -= bulletCount > 1 ? d * bulletCount / 2 : 0;

        for (var x = 0; x < bulletCount; x++)
        {
            var dx = MathF.Cos(direction + (d * x));
            var dy = MathF.Sin(direction + (d * x));

            var bulletX = -dx + rigidBody.Position.X;
            var bulletY = -dy + rigidBody.Position.Y;
            var bulletPos = new Vector2(bulletX, bulletY);

            var dist = rigidBody.Position - bulletPos;
            var entitySize = 1.0f; // Fixed 1x1 entity size
            var penDepth = entitySize - wep.BulletSize - dist.Length();
            var penRes = Vector2.Normalize(dist) * penDepth;
            bulletPos += penRes * 1.25f;

            if (bulletPos.X + (wep.BulletSize / 2) > Game.MapSize.X || bulletPos.X - (wep.BulletSize / 2) < 0 || bulletPos.Y + (wep.BulletSize / 2) > Game.MapSize.Y || bulletPos.Y - (wep.BulletSize / 2) < 0)
                continue;
            var velocity = new Vector2(dx, dy) * wep.BulletSpeed;
            SpawnManager.SpawnBullets(in ntt, ref bulletPos, ref wep, ref velocity, Convert.ToUInt32("80ED99", 16));
        }
    }
}