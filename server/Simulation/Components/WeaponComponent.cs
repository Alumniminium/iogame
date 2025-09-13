using System;
using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components;

[Component]
public struct WeaponComponent(int entityId, float directionDeg, byte bulletDamage, byte bulletCount, byte bulletSize, byte bulletSpeed, float powerUse, TimeSpan frequency)
{
    public readonly int EntityId = entityId;
    public bool Fire = false;
    public TimeSpan Frequency = frequency;
    public TimeSpan LastShot = TimeSpan.Zero;
    public ushort BulletDamage = bulletDamage;
    public byte BulletCount = bulletCount;
    public byte BulletSize = bulletSize;
    public ushort BulletSpeed = bulletSpeed;
    public float PowerUse = powerUse;
    public Vector2 Direction = directionDeg.AsVectorFromDegrees();

    public override int GetHashCode() => EntityId;
}