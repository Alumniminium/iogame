namespace server.Simulation.Database;

public sealed class BaseResource
{
    public int Sides;
    public uint Color;
    public uint BorderColor;
    public int Health;
    public float Density;
    public float Elasticity;
    public float Drag;
    public int MaxSpeed;
    public int MaxAliveNum;

    public BaseResource() { }
    public BaseResource(int sides, uint color, uint borderColor, float elasticity, float drag, int health, int maxAliveNum)
    {
        Sides = sides;
        Color = color;
        BorderColor = borderColor;
        Health = health;
        MaxAliveNum = maxAliveNum;
        Drag = drag;
        Elasticity = elasticity;
        MaxSpeed = 1500;
    }

}