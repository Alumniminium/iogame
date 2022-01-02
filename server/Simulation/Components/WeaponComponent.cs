using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct WeaponComponent
    {
        public uint LastShot;
        public Vector2 Direction;
        public byte BulletCount;

        public WeaponComponent(float direction)
        {
            BulletCount = 1;
            LastShot = 0;
            Direction = direction.FromRadians();
        }
        
        public WeaponComponent(Vector2 direction)
        {
            BulletCount = 1;
            LastShot = 0;
            Direction = direction;
        }
    }
}