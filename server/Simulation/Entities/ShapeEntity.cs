using server.ECS;

namespace server.Simulation.Entities
{
    public class ShapeEntity
    {
        public PixelEntity Entity;
        public PixelEntity Owner;
    }
    public class Player : ShapeEntity { }
    public class Bullet : ShapeEntity { }
    public class Boid : ShapeEntity { }
}