using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class PickupCollisionResolver : NttSystem<CollisionComponent, InventoryComponent>
{
    public PickupCollisionResolver() : base("Pickup Collision Resolver", threads: 1) { }
    protected override bool MatchesFilter(in NTT ntt) => ntt.Has<NetworkComponent>() && base.MatchesFilter(ntt);

    public override void Update(in NTT ntt, ref CollisionComponent col, ref InventoryComponent inv)
    {
        FConsole.WriteLine($"🎒 PickupCollisionResolver processing player {ntt.Id} with {col.Collisions.Count} collisions");

        for (int x = 0; x < col.Collisions.Count; x++)
        {
            var b = col.Collisions[x].Item1;

            if (inv.TotalCapacity == inv.Triangles + inv.Squares + inv.Pentagons)
                return;

            FConsole.WriteLine($"🔍 Checking collision with entity {b.Id}: HasPickable={b.Has<PickableTagComponent>()}, HasBody={b.Has<Box2DBodyComponent>()}");

            // Only process entities that are marked as pickable
            if (!b.Has<PickableTagComponent>() || !b.Has<Box2DBodyComponent>())
                continue;

            ref readonly var physicsComp = ref b.Get<Box2DBodyComponent>();

            if (physicsComp.Sides == 3)
                inv.Triangles++;
            else if (physicsComp.Sides == 4)
                inv.Squares++;
            else if (physicsComp.Sides == 5)
                inv.Pentagons++;

            inv.ChangedTick = NttWorld.Tick;
            var dtc = new DeathTagComponent();
            b.Set(ref dtc);
            FConsole.WriteLine($"✅ Picked up drop {b.Id} with {physicsComp.Sides} sides - marked for deletion");
        }
    }
}