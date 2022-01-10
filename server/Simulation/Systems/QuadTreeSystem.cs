using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class QuadTreeSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public readonly ConcurrentStack<ShapeEntity> MovedEntitiesThisFrame = new();
        public QuadTreeSystem() : base("QuadTree System", threads: Environment.ProcessorCount) { }

        protected override void PreUpdate() => MovedEntitiesThisFrame.Clear();

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            if (phy.AngularVelocity == 0 && phy.Acceleration == Vector2.Zero && phy.Velocity == Vector2.Zero)
                return;

            var shpEntity = PixelWorld.GetAttachedShapeEntity(in ntt);
            MovedEntitiesThisFrame.Push(shpEntity);
            var rect = shpEntity.Rect;
            rect.X = Math.Clamp(phy.Position.X - shpEntity.Rect.Width / 2, shpEntity.Rect.Width / 2, Game.MapSize.X-shpEntity.Rect.Width / 2);
            rect.Y = Math.Clamp(phy.Position.Y - shpEntity.Rect.Height / 2, shpEntity.Rect.Height / 2, Game.MapSize.Y-shpEntity.Rect.Height / 2);
            shpEntity.Rect = rect;
        }

        protected override void PostUpdate()
        {
            while (MovedEntitiesThisFrame.TryPop(out var ntt))
                if (!Game.Tree.Move(ntt))
                {
                    ntt.Rect = new RectangleF(2, 2, 1, 1);
                    PixelWorld.Destroy(in ntt.Entity);
                }
        }
    }
}