using System;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Weapon, NetworkSync = true)]
public struct WeaponComponent(NTT owner, float directionDeg, byte bulletDamage, byte bulletCount, byte bulletSize, byte bulletSpeed, float powerUse, TimeSpan frequency)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly NTT Owner = owner;
    public bool Fire = false;
    public TimeSpan Frequency = frequency;
    public TimeSpan LastShot = TimeSpan.Zero;
    public ushort BulletDamage = bulletDamage;
    public byte BulletCount = bulletCount;
    public byte BulletSize = bulletSize;
    public ushort BulletSpeed = bulletSpeed;
    public float PowerUse = powerUse;
    public Vector2 Direction = directionDeg.AsVectorFromDegrees();


}