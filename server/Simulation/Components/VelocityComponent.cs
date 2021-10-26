using System.Numerics;
namespace iogame.Simulation.Components
{
    public struct VelocityComponent
    {
        public Vector2 Movement;
        public float Spin = 0;
        public Vector2 Acceleration = Vector2.Zero;
        public uint MaxSpeed;

        public VelocityComponent(float x, float y, uint maxSpeed)
        {
            Movement = new Vector2(x, y);
            MaxSpeed = maxSpeed;
        }

        public void Deconstruct(out Vector2 vel, out float spin, out Vector2 accel)
        {
            vel = Movement;
            spin = Spin;
            accel = Acceleration;
        }
    }
}