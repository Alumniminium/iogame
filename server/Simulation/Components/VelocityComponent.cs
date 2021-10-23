using System.Numerics;
namespace iogame.Simulation.Components
{
    public class VelocityComponent : GameComponent
    {
        public Vector2 Movement;
        public float Spin;
        public Vector2 Acceleration;
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