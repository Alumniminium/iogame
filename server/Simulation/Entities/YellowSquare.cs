using System.Numerics;
using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{
    public class YellowSquare : Entity
    {
        public unsafe YellowSquare()
        {
            VelocityComponent = new VelocityComponent(0, 0, maxSpeed: 1500);
            ShapeComponent = new ShapeComponent(4, 100);
            HealthComponent = new HealthComponent(1000, 1000, 0);
            var mass = (float)Math.Pow(ShapeComponent.Size, 3);
            PhysicsComponent = new PhysicsComponent(mass);
            // FillColor = Convert.ToUInt32("ffe869", 16);
            // BorderColor = Convert.ToUInt32("bfae4e", 16);
        }
    }
}