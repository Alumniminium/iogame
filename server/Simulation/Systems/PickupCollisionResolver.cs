using System;
using Packets.Enums;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class PickupCollisionResolver : PixelSystem<CollisionComponent, InventoryComponent>
    {
        public PickupCollisionResolver() : base("Pickup Collision Resolver", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => (ntt.Type == EntityType.Player || ntt.Type == EntityType.Npc) && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref CollisionComponent col, ref InventoryComponent inv)
        {
            for (int x = 0; x < col.Collisions.Count; x++)
            {
                var b = col.Collisions[x].Item1;

                if (b.Type != EntityType.Pickable)
                    return;

                if (inv.TotalCapacity == inv.Triangles + inv.Squares + inv.Pentagons)
                    return;

                ref readonly var phy = ref b.Get<PhysicsComponent>();

                if (phy.Color == Convert.ToUInt32("80ED99", 16))
                {
                    inv.Triangles++;
                }
                else if (phy.Color == Convert.ToUInt32("DB5461", 16))
                {
                    inv.Squares++;
                }
                else if (phy.Color == Convert.ToUInt32("6F2DBD", 16))
                {
                    inv.Pentagons++;
                }
                inv.ChangedTick = Game.CurrentTick;
                var dtc = new DeathTagComponent();
                b.Add(ref dtc);
            }
        }
    }
}