using System;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;

namespace iogame.Simulation.Database
{
    public class BaseResource
    {
        public int Sides;
        public int Size;
        public uint Color;
        public uint BorderColor;
        public float Mass;
        public int Health;
        public int BodyDamage;

        public BaseResource(){}
        public BaseResource(int sides, int size, uint color, uint borderColor, float mass, int health, int bodyDamage)
        {
            Sides = sides;
            Size = size;
            Color = color;
            BorderColor = borderColor;
            Mass = mass;
            Health = health;
            BodyDamage = bodyDamage;
        }
    }
}