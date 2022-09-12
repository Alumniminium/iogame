namespace server.Simulation.Database
{
    public class BaseResource
    {
        public int Sides { get; set; }
        public uint Color { get; set; }
        public uint BorderColor { get; set; }
        public int Health { get; set; }
        public int BodyDamage { get; set; }

        public int Size { get; set; }
        public float Mass { get; set; }
        public float Elasticity { get; set; }
        public float Drag { get; set; }
        public int MaxSpeed { get; set; }

        public int MaxAliveNum { get; set; }

        public BaseResource() { }
        public BaseResource(int sides, int size, uint color, uint borderColor, float mass, float elasticity, float drag, int health, int bodyDamage, int maxAliveNum)
        {
            Sides = sides;
            Size = size;
            Color = color;
            BorderColor = borderColor;
            Mass = mass;
            Health = health;
            BodyDamage = bodyDamage;
            MaxAliveNum = maxAliveNum;
            Drag = drag;
            Elasticity = elasticity;
            MaxSpeed = 1500;
        }

    }
}