namespace server.Simulation.Database
{
    public sealed class BaseResource
    {
        public int Sides;
        public uint Color;
        public uint BorderColor;
        public int Health;
        public int BodyDamage;
        public int Size;
        public float Mass;
        public float Elasticity;
        public float Drag;
        public int MaxSpeed;
        public int MaxAliveNum;

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