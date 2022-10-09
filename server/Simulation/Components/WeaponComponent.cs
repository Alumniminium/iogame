using System;
using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct WeaponComponent
    {
        public readonly int EntityId;
        public bool Fire;
        public TimeSpan Frequency;
        public TimeSpan LastShot;
        public ushort BulletDamage;
        public byte BulletCount;
        public byte BulletSize;
        public ushort BulletSpeed;
        public float PowerUse;
        public Vector2 Direction;

        public WeaponComponent(int entityId, float directionDeg, byte bulletDamage, byte bulletCount, byte bulletSize, byte bulletSpeed, float powerUse, TimeSpan frequency)
        {
            EntityId = entityId;
            Fire = false;
            Frequency = frequency;
            LastShot = TimeSpan.Zero;
            BulletDamage = bulletDamage;
            BulletCount = bulletCount;
            BulletSize = bulletSize;
            BulletSpeed = bulletSpeed;
            PowerUse = powerUse;
            Direction = directionDeg.AsVectorFromDegrees();
        }
        public override int GetHashCode() => EntityId;
    }
}