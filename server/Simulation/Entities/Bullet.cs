using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{
    public unsafe class Bullet : Entity
    {
        public Entity Owner;
        public float LifeTimeSeconds;

        public Bullet()
        {
            VelocityComponent = new VelocityComponent(0, 0, maxSpeed: 1500);
            ShapeComponent = new ShapeComponent(sides: 0, size: 25);
            HealthComponent = new HealthComponent(20,20,0);

            var mass = (float)Math.Pow(ShapeComponent.Size, 3);
            PhysicsComponent = new PhysicsComponent(mass, 0,0);
            
            // FillColor = Convert.ToUInt32("ffe869", 16);
            // BorderColor = Convert.ToUInt32("bfae4e", 16);
            BodyDamage = 2f;
        }

        public void SetOwner(Entity owner) => Owner = owner;
    }
}