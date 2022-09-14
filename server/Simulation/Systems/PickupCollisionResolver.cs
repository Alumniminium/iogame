using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class PickupCollisionResolver : PixelSystem<CollisionComponent, InventoryComponent>
    {
        public PickupCollisionResolver() : base("Pickup Collision Resolver", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => (ntt.Type == EntityType.Player || ntt.Type == EntityType.Npc) && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref CollisionComponent col, ref InventoryComponent inv)
        {
            var b = ntt.Id == col.A.Id ? col.B : col.A;

            if (inv.TotalCapacity == inv.Triangles + inv.Squares + inv.Pentagons)
                return;
            if (b.Type != EntityType.Pickable)
                return;

            ref var shp = ref b.Get<ShapeComponent>();

            switch (shp.Sides)
            {
                case 3:
                    inv.Triangles++;
                    break;
                case 4:
                    inv.Squares++;
                    break;
                case 5:
                    inv.Pentagons++;
                    break;
            }

            inv.ChangedTick = Game.CurrentTick;
            PixelWorld.Destroy(in b);
        }
    }
}