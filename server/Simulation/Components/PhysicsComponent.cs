using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct PhysicsComponent
    {
        public readonly float Mass;
        public readonly float InverseMass => 1f / Mass;
        public readonly float Elasticity;
        public readonly float Drag;

        public PhysicsComponent(float mass, float elasticity = 1, float drag = 0f)
        {
            Mass = mass;
            Elasticity = elasticity;
            Drag = drag;
        }
    }
}