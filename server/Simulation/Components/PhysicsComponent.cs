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
        public float Rotation;
        public float AngularVelocity;
        public Vector2 Position;
        public Vector2 LastPosition;
        public Vector2 LastSyncedPosition;
        public Vector2 Acceleration;
        public Vector2 Velocity;

        public Vector2 Forward
        {
            get
            {
                var dx = (float)Math.Cos(Rotation);
                var dy = (float)Math.Sin(Rotation);
                return Vector2.Normalize(new Vector2(dx, dy));
            }
        }

        public PhysicsComponent(Vector2 position, float mass, float elasticity = 1, float drag = 0f)
        {
            AngularVelocity=0;
            Mass = mass;
            Elasticity = elasticity;
            Drag = drag;
            Acceleration = Vector2.Zero;
            Velocity = Vector2.Zero;
            Position = position;
            LastPosition = position;
            LastSyncedPosition = Vector2.Zero;
            Rotation = 0f;
        }
    }
}