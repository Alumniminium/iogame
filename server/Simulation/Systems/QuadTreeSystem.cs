using System;
using System.Collections.Concurrent;
using System.Drawing;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class QuadTreeSystem : PixelSystem<PhysicsComponent>
    {
        public readonly ConcurrentStack<ShapeEntity> MovedEntitiesThisFrame = new();
        public QuadTreeSystem() : base("QuadTree System", threads: Environment.ProcessorCount) { }

        protected override void PreUpdate()
        {
            MovedEntitiesThisFrame.Clear();
        }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            if (phy.Position == phy.LastPosition)
                return;

            var shpEntity = PixelWorld.GetAttachedShapeEntity(in ntt);
            MovedEntitiesThisFrame.Push(shpEntity);
            var rect = shpEntity.Rect;
            rect.X = Math.Clamp(1 + phy.Position.X - shpEntity.Rect.Width / 2, shpEntity.Rect.Width / 2, Game.MapSize.X - 1 - shpEntity.Rect.Width / 2);
            rect.Y = Math.Clamp(1 + phy.Position.Y - shpEntity.Rect.Height / 2, shpEntity.Rect.Height / 2, Game.MapSize.Y - 1 - shpEntity.Rect.Height / 2);
            shpEntity.Rect = rect;
        }

        protected override void PostUpdate()
        {
            while (MovedEntitiesThisFrame.TryPop(out var ntt))
            {
                if (!Game.Tree.Move(ntt))
                {
                    ntt.Rect = new RectangleF(2, 2, 1, 1);
                    PixelWorld.Destroy(in ntt.Entity);
                }
            }
        }
    }
}