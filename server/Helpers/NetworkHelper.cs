using System;
using System.Collections.Generic;
using server.ECS;
using server.Enums;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Helpers;

public static class NetworkHelper
{
    public static void FullSync(NTT to, NTT ntt)
    {
        if (!ntt.Has<Box2DBodyComponent>())
            return;

        ref readonly var physics = ref ntt.Get<Box2DBodyComponent>();

        uint color = physics.Color; // Get color from Box2DBodyComponent

        // Create spawn packet with parts (either from ship configuration or default single part)
        SpawnPacket spawnPacket;
        if (ntt.Has<ShipConfigurationComponent>())
        {
            ref readonly var shipConfig = ref ntt.Get<ShipConfigurationComponent>();
            spawnPacket = SpawnPacket.Create(ntt, physics.ShapeType, physics.Position, physics.RotationRadians, color, shipConfig.Parts, shipConfig.CenterX, shipConfig.CenterY);
        }
        else
        {
            // Create default single part for entities without custom configuration
            var defaultParts = new List<ShipPart>
            {
                new ShipPart(0, 0, 0, (byte)physics.ShapeType, 0) // Single part at center
            };
            spawnPacket = SpawnPacket.Create(ntt, physics.ShapeType, physics.Position, physics.RotationRadians, color, defaultParts, 0, 0);
        }

        Memory<byte> buffer = spawnPacket.ToBuffer();
        to.NetSync(buffer);

        if (ntt.Has<ShieldComponent>())
        {
            ref readonly var shi = ref ntt.Get<ShieldComponent>();
            Memory<byte> shieldPacket = StatusPacket.Create(ntt, shi.Charge, StatusType.ShieldCharge);
            to.NetSync(shieldPacket);
        }

        if (ntt.Has<HealthComponent>())
        {
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            Memory<byte> healthPacket = StatusPacket.Create(ntt, hlt.Health, StatusType.Health);
            to.NetSync(healthPacket);
        }
    }
}