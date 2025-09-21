using System;
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

        Memory<byte> spawnPacket = SpawnPacket.Create(ntt, physics.ShapeType, physics.Width, physics.Height, physics.Position, physics.RotationRadians, color);
        to.NetSync(spawnPacket);

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