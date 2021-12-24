using System.Drawing;
using QuadTrees.QTreeRectF;
using server.ECS;

namespace server.Simulation.Entities
{
    public class ShapeEntity : IRectFQuadStorable
    {
        public PixelEntity Entity;
        public RectangleF Rect {get;set;} = RectangleF.Empty;
    }
    public class Player : ShapeEntity { }
    public class Bullet : ShapeEntity { }
    public class Boid : ShapeEntity { }
    public class Structure : ShapeEntity { }
}