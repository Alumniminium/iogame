using System.Drawing;
using QuadTrees.QTreeRectF;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Entities
{
    public class ShapeEntity : IRectFQuadStorable
    {
        public PixelEntity Entity;
        public PixelEntity Owner;
        public ref PositionComponent PositionComponent => ref Entity.Get<PositionComponent>();
        public ref ShapeComponent ShapeComponent => ref Entity.Get<ShapeComponent>();

        public RectangleF Rect => new(PositionComponent.Position.X - ShapeComponent.Radius, PositionComponent.Position.Y - ShapeComponent.Radius, ShapeComponent.Radius, ShapeComponent.Radius);
    }
    public class Player : ShapeEntity { }
    public class Bullet : ShapeEntity { }
    public class Boid : ShapeEntity { }
}