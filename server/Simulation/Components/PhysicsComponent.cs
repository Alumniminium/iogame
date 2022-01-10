using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct PhysicsComponent
    {
        public float Mass;
        public float InverseMass => 1f / Mass;
        public float Elasticity;
        public float Drag;
        public float RotationRadians;
        public float AngularVelocity;
        public Vector2 LastPosition;
        public Vector2 Position;
        public Vector2 Acceleration;
        public Vector2 Velocity;

        public Vector2 Forward => RotationRadians.AsVectorFromRadians();

        public uint ChangedTick;

        public PhysicsComponent(Vector2 position, float mass, float elasticity = 1, float drag = 0f)
        {
            ChangedTick = 0;
            AngularVelocity=0;
            Mass = mass;
            Elasticity = elasticity;
            Drag = drag;
            Acceleration = Vector2.Zero;
            Velocity = Vector2.Zero;
            Position = position;
            LastPosition = position;
            RotationRadians = 0f;
        }
    }
}