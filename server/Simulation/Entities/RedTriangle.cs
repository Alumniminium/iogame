using System.Numerics;
using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{
    public class RedTriangle : Entity
    {
        public RedTriangle()
        {
            PositionComponent = new PositionComponent(0, 0);
            VelocityComponent = new VelocityComponent(0, 0, maxSpeed: 1500);
            ShapeComponent = new ShapeComponent(3,200);
            HealthComponent = new HealthComponent(1000,1000,0);
            var mass = (float)Math.Pow(ShapeComponent.Size, 3);
            PhysicsComponent = new PhysicsComponent(mass);
            // FillColor = Convert.ToUInt32("ff5050", 16);
            // BorderColor = Convert.ToUInt32("ff9999", 16);
        }
    }
}