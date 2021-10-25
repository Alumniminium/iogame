namespace iogame.Simulation.Components
{
    public struct PhysicsComponent
    {
        public float Mass;
        public float InverseMass => 1f / Mass;
        public float Elasticity;
        public float Drag = Game.DRAG;

        public PhysicsComponent(float mass, float elasticity = 1, float drag = Game.DRAG)
        {
            Mass = mass;
            Elasticity = elasticity;
            Drag = drag;
        }
    }
}