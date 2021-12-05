using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct PhysicsComponent
    {
        public float Mass;
        public float InverseMass => 1f / Mass;
        public float Elasticity;
        public float Drag;

        public PhysicsComponent(float mass, float elasticity = 1, float drag = 0f)
        {
            Mass = mass;
            Elasticity = elasticity;
            Drag = drag;
        }
    }
}