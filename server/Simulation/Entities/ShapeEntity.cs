using System.Drawing;
using QuadTrees.QTreeRect;
using QuadTrees.QTreeRectF;
using server.ECS;

namespace server.Simulation.Entities
{
    public class ShapeEntity : IRectQuadStorable
    {
        public PixelEntity Entity;
        public Rectangle Rect {get;set;} = Rectangle.Empty;
    }
    public class Player : ShapeEntity { }
    public class Bullet : ShapeEntity { }
    public class Boid : ShapeEntity { }
    public class Structure : ShapeEntity { }
    public class Drop : ShapeEntity { }
}