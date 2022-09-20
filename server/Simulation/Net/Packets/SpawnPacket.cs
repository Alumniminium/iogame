using System;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Net.Packets
{
    public static unsafe class SpawnPacket
    {
        public static Memory<byte> Create(in PixelEntity ntt)
        {
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            if (phy.ShapeType == ShapeType.Box)
                return BoxSpawnPacket.Create(ntt);
            else
                return SphereSpawnPacket.Create(in ntt);
        }
    }
}