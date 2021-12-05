using System.Drawing;
using QuadTrees.QTreeRectF;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct  ColliderComponent : IRectFQuadStorable
    {
        public int EntityId;
        public RectangleF Rect { get;set;}
    }
}