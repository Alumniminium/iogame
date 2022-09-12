using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class PickupCollisionResolver : PixelSystem<CollisionComponent, InventoryComponent>
    {
        public PickupCollisionResolver() : base("Pickup Collision Resolver", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt)
        {
            return (ntt.Type == EntityType.Player || ntt.Type == EntityType.Npc) && base.MatchesFilter(ntt);
        }

        public override void Update(in PixelEntity ntt, ref CollisionComponent c1, ref InventoryComponent c2)
        {
            if (c2.TotalCapacity == c2.Triangles + c2.Squares + c2.Pentagons)
                return;

            var b = ntt.Id == c1.A.Id ? c1.B : c1.A;

            if (b.Type != EntityType.Drop)
                return;

            ref var shp = ref b.Get<ShapeComponent>();

            switch (shp.Sides)
            {
                case 3:
                    c2.Triangles++;
                    break;
                case 4:
                    c2.Squares++;
                    break;
                case 5:
                    c2.Pentagons++;
                    break;
            }

            c2.ChangedTick = Game.CurrentTick;
            PixelWorld.Destroy(in b);
            ntt.Remove<CollisionComponent>();
        }
    }
}