using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct WeaponComponent
    {
        public uint LastShot;
        public float Direction;

        public WeaponComponent(float direction)
        {
            LastShot = 0;
            Direction = direction;
        }
    }
}