using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class PickupCollisionResolver : PixelSystem<CollisionComponent, InventoryComponent>
    {
        public PickupCollisionResolver() : base("Pickup Collision Resolver", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt)
        {
            return (ntt.IsPlayer() || ntt.IsNpc()) && base.MatchesFilter(ntt);
        }

        public override void Update(in PixelEntity a, ref CollisionComponent col, ref InventoryComponent inv)
        {
            if (inv.TotalCapacity == inv.Triangles + inv.Squares + inv.Pentagons)
                return;

            var b = a.Id == col.A.Id ? col.B : col.A;

            if (!b.IsDrop())
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
            a.Remove<CollisionComponent>();
        }
    }
}