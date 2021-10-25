using System.Numerics;
using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{
    public class PurplePentagon : Entity
    {
        public unsafe PurplePentagon()
        {
            VelocityComponent = new VelocityComponent(0, 0, maxSpeed: 1500);
            ShapeComponent = new Components.ShapeComponent(5,300);
            HealthComponent = new HealthComponent(1000,1000,0);
            var mass = (float)Math.Pow(ShapeComponent.Size, 3);
            PhysicsComponent = new PhysicsComponent(mass);
            // FillColor = Convert.ToUInt32("4B0082", 16);
            // BorderColor = Convert.ToUInt32("9370DB", 16);
        }
    }
}