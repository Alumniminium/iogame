using System.Drawing;
using System.Reflection.Metadata;
using iogame.ECS;
using QuadTrees.QTreeRectF;

namespace iogame.Simulation.Components
{
    [Component]
    public struct BoidComponent
    {
        public int Flock = -1;
    }

    [Component]
    public struct  ColliderComponent : IRectFQuadStorable
    {
        public int EntityId;
        public RectangleF Rect { get;set;}
    }
}