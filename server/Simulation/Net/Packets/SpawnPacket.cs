using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public static unsafe class SpawnPacket
    {
        public static byte[] Create(in PixelEntity ntt)
        {
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            if (phy.ShapeType == ShapeType.Box)
            {
                return BoxSpawnPacket.Create(ntt);
            }
            else
            {
                return SphereSpawnPacket.Create(in ntt);
            }
        }
    }
}