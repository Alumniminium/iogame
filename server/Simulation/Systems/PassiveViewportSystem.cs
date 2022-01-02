using System.Collections.Concurrent;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class PassiveViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public readonly ConcurrentStack<ShapeEntity> MovedEntitiesThisFrame = new();
        public PassiveViewportSystem() : base("Passive Viewport Sys", threads: Environment.ProcessorCount) { }

        protected override void PreUpdate() => MovedEntitiesThisFrame.Clear();
        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            vwp.EntitiesVisibleLastSync.Clear();
            vwp.EntitiesVisibleLastSync.AddRange(vwp.EntitiesVisible);

            if(phy.Position == phy.LastPosition)
                return;

            var shpEntity = PixelWorld.GetAttachedShapeEntity(in ntt);
            MovedEntitiesThisFrame.Push(shpEntity);
            var rect = shpEntity.Rect;
            rect.X = (int)phy.Position.X - shpEntity.Rect.Width / 2;
            rect.Y = (int)phy.Position.Y - shpEntity.Rect.Height / 2;
            shpEntity.Rect = rect;

            vwp.Viewport.X = (int)phy.Position.X - vwp.ViewDistance / 2;
            vwp.Viewport.Y = (int)phy.Position.Y - vwp.ViewDistance / 2;
        }
        protected override void PostUpdate()
        {
            while(MovedEntitiesThisFrame.TryPop(out var ntt))
                Game.Tree.Move(ntt);
        }
    }
}