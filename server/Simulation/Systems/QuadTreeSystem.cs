using System;
using System.Collections.Concurrent;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class SpacePartitionSystem : PixelSystem<PhysicsComponent>
    {
        public readonly ConcurrentStack<PixelEntity> MovedEntitiesThisFrame = new();
        public SpacePartitionSystem() : base("QuadTree System", threads: Environment.ProcessorCount) { }

        protected override void PreUpdate()
        {
            MovedEntitiesThisFrame.Clear();
        }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            if (phy.Position == phy.LastPosition)
                return;

            MovedEntitiesThisFrame.Push(ntt);
        }

        protected override void PostUpdate()
        {
            while (MovedEntitiesThisFrame.TryPop(out var ntt))
                Game.Grid.Move(ntt);
        }
    }
}