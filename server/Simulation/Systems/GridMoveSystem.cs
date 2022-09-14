using System.Collections.Concurrent;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class GridMoveSystem : PixelSystem<PhysicsComponent,ViewportComponent>
    {
        public readonly ConcurrentStack<PixelEntity> MovedEntitiesThisFrame = new();
        public GridMoveSystem() : base("Grid Move System", threads: 1) { }

        protected override void PreUpdate() => MovedEntitiesThisFrame.Clear();

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            if(phy.ChangedTick != Game.CurrentTick)
                return;
                
            MovedEntitiesThisFrame.Push(ntt);
            var rect = vwp.Viewport;
            rect.X = (int)phy.Position.X - rect.Width / 2;
            rect.Y = (int)phy.Position.Y - rect.Height / 2;
            vwp.Viewport = rect;
        }

        protected override void PostUpdate()
        {            
            while(MovedEntitiesThisFrame.TryPop(out var ntt))
                Game.Grid.Move(ntt);
        }
    }
}