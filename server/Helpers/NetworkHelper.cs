using System;
using Packets;
using Packets.Enums;
using server.ECS;
using server.Simulation.Components;

namespace server
{
    public static class NetworkHelper
    {
        public static void FullSync(PixelEntity to, PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();
            Memory<byte> spawnPacket = SpawnPacket.Create(ntt.Id, phy.ShapeType, phy.Radius, phy.Width, phy.Height, phy.Position, phy.RotationRadians, phy.Color);
            to.NetSync(spawnPacket);
            
            if(ntt.Has<ShieldComponent>())
            {
                ref readonly var shi = ref ntt.Get<ShieldComponent>();
                Memory<byte> shieldPacket = StatusPacket.Create(ntt.Id, shi.Charge,StatusType.ShieldCharge);
                to.NetSync(shieldPacket);
            }

            if(ntt.Has<HealthComponent>())
            {
                ref readonly var hlt = ref ntt.Get<HealthComponent>();
                Memory<byte> healthPacket = StatusPacket.Create(ntt.Id, hlt.Health, StatusType.Health);
                to.NetSync(healthPacket);
            }

        }
    }
}