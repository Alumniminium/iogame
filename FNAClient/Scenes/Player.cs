using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RG351MP.Helpers;
using server.Simulation.Database;
using static RG351MP.Helpers.PolygonHelper;

namespace RG351MP.Scenes
{
    public class Player : Entity
    {
        public Player(int uniqueId, ShapeType shapeType, Vector2 position, float width, float height, float direction, uint color) : base(uniqueId, shapeType, position, width, height, direction, color)
        {
        }
    }
    public class Entity
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Size;
        public Polygon Polygon;
        public int UniqueId;
        public ShapeType shapeType;
        public float width;
        public float height;
        public float direction;
        public uint color;

        public Entity(int uniqueId, ShapeType shapeType, Vector2 position, float width, float height, float direction, uint color)
        {
            this.UniqueId = uniqueId;
            this.shapeType = shapeType;
            Position = position;
            this.width = width;
            this.height = height;
            this.direction = direction;
            this.color = color;
            Polygon = new Polygon(PolygonHelper.GenerateShape(4,width, Color.AliceBlue));
        }
    }
}