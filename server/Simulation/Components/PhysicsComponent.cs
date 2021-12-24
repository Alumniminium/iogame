using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct PhysicsComponent
    {
        public float Mass;
        public float InverseMass => 1f / Mass;
        public float Elasticity;
        public float Drag;

        public Vector2 Acceleration;
        public Vector2 Velocity;

        public PhysicsComponent(float mass, float elasticity = 1, float drag = 0f)
        {
            Mass = mass;
            Elasticity = elasticity;
            Drag = drag;
            Acceleration = Vector2.Zero;
            Velocity = Vector2.Zero;
        }
    }
}