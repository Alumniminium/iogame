using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class PassiveViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public readonly RefList<ShapeEntity> MovedEntitiesThisFrame = new();
        public PassiveViewportSystem() : base("Passive Viewport Sys", threads: 12) { }

        protected override void PreUpdate() => MovedEntitiesThisFrame.Clear();
        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            vwp.EntitiesVisibleLastSync.Clear();
            vwp.EntitiesVisibleLastSync.AddRange(vwp.EntitiesVisible);

            var shpEntity = PixelWorld.GetAttachedShapeEntity(in ntt);
            MovedEntitiesThisFrame.Add(shpEntity);
            var rect = shpEntity.Rect;
            rect.X = phy.Position.X - shpEntity.Rect.Width / 2;
            rect.X = phy.Position.Y - shpEntity.Rect.Height / 2;
            shpEntity.Rect = rect;

            vwp.Viewport.X = phy.Position.X - vwp.ViewDistance / 2;
            vwp.Viewport.Y = phy.Position.Y - vwp.ViewDistance / 2;
            vwp.Viewport.X = Math.Clamp(vwp.Viewport.X, 0, Game.MapSize.X);
            vwp.Viewport.Y = Math.Clamp(vwp.Viewport.Y, 0, Game.MapSize.Y);
        }
        protected override void PostUpdate()
        {
            for (var i = 0; i < MovedEntitiesThisFrame.Count; i++)
            {
                ref readonly var ntt = ref MovedEntitiesThisFrame[i];
                Game.Tree.Move(ntt);
            }
        }
    }
}