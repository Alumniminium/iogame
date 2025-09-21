using System;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class Box2DSyncSystem : NttSystem<Box2DBodyComponent>
{
    public Box2DSyncSystem() : base("Box2D Sync System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent body)
    {
        // Store previous velocity for fall damage calculation
        var previousVelocity = body.LinearVelocity;

        // Sync Box2D transform back to component every frame for movement detection
        if (body.IsValid)
        {
            body.SyncFromBox2D();
        }

        // Calculate fall damage from violent velocity changes
        if (ntt.Has<HealthComponent>() && !body.IsStatic)
        {
            var velocityChange = body.LinearVelocity - previousVelocity;
            var impactMagnitude = velocityChange.Length();

            // Apply fall damage if impact is severe enough
            const float damageThreshold = 15f; // Minimum impact speed for damage
            const float damageMultiplier = 2f; // Damage per unit of impact speed

            if (impactMagnitude > damageThreshold)
            {
                var fallDamage = (impactMagnitude - damageThreshold) * damageMultiplier;

                // Only apply significant damage (> 1 point)
                if (fallDamage > 1f)
                {
                    var damageComponent = new DamageComponent(ntt, ntt, fallDamage); // Self-inflicted damage
                    ntt.Set(ref damageComponent);
                }
            }
        }

        // Update spatial grid if entity moved significantly
        if (body.LastSyncTick < NttWorld.Tick - 1) // Update every few ticks
        {
            // TODO: Update the spatial grid for gameplay queries (non-physics)
            // Game.Grid.Update(in ntt, body.Position);
            body.LastSyncTick = NttWorld.Tick;
        }
    }
}