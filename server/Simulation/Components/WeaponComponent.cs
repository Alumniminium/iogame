using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct WeaponComponent
    {
        public bool Fire;
        public uint LastShot;
        public Vector2 Direction;
        public byte BulletCount;
        public byte BulletSize;
        public byte BulletSpeed;
        public float PowerUse;

        public WeaponComponent(float directionDeg)
        {
            Fire = false;
            BulletCount = 1;
            LastShot = 0;
            BulletSize = 2;
            BulletSpeed = 50;
            PowerUse = 100f;
            Direction = directionDeg.AsVectorFromDegrees();
        }

        public WeaponComponent(Vector2 direction)
        {
            Fire = false;
            BulletCount = 1;
            LastShot = 0;
            BulletSize = 2;
            BulletSpeed = 50;
            PowerUse = 100f;
            Direction = direction;
        }
    }
}