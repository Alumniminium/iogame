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
        ref readonly var phy = ref ntt.Get<PhysicsComponent>();
        Memory<byte> spawnPacket = SpawnPacket.Create(ntt, phy.ShapeType, phy.Radius, phy.Width, phy.Height, phy.Position, phy.RotationRadians, phy.Color);
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