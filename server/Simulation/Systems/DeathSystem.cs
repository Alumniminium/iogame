using System;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public sealed class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent c1)
        {
            if (ntt.Has<ShapeComponent>())
            {
                ref readonly var shp = ref ntt.Get<ShapeComponent>();
                if(SpawnManager.MapResources.ContainsKey(shp.Sides))
                    SpawnManager.MapResources[shp.Sides]--;
            }
            if(ntt.Has<PhysicsComponent>())
            {
                ref var phy = ref ntt.Get<PhysicsComponent>();
                Game.Grid.Remove(in ntt, ref phy);
            }
            PixelWorld.Destroy(in ntt);
        }
    }
}