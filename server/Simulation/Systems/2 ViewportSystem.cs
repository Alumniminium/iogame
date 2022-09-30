using System.Runtime.CompilerServices;
using Packets;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public unsafe sealed class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Pickable && ntt.Type != EntityType.Static && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            if (phy.LastPosition == phy.Position && ntt.Type != EntityType.Player)
                return;
            
            vwp.Viewport.X = phy.Position.X - vwp.Viewport.Width / 2;
            vwp.Viewport.Y = phy.Position.Y - vwp.Viewport.Height / 2;

            var handle = vwp.EntitiesVisibleLast.Pin();
            Unsafe.InitBlockUnaligned(handle.Pointer, 0, (uint) vwp.EntitiesVisibleLast.Length);
            handle.Dispose();

            vwp.EntitiesVisible.CopyTo(vwp.EntitiesVisibleLast);
            handle = vwp.EntitiesVisible.Pin();
            Unsafe.InitBlockUnaligned(handle.Pointer, 0, (uint) vwp.EntitiesVisible.Length);
            handle.Dispose();

            Game.Grid.GetVisibleEntities(ref vwp);

            if (ntt.Type != EntityType.Player)
                return;

            // despawn entities not visible anymore and spawn new ones

            for (var i = 0; i < vwp.EntitiesVisibleLast.Length; i++)
            {
                var b = vwp.EntitiesVisibleLast.Span[i];
                var found = false;
                if (ntt.Id == b.Id)
                    continue;

                for (var j = 0; j < vwp.EntitiesVisible.Length; j++)
                {
                    found = vwp.EntitiesVisible.Span[j].Id == b.Id;
                    if (found)
                        break;
                }

                if (found)
                    continue;

                ntt.NetSync(StatusPacket.CreateDespawn(b.Id));
            }

            for (var i = 0; i < vwp.EntitiesVisible.Length; i++)
            {
                var b = vwp.EntitiesVisible.Span[i];
                var found = false;

                for (var j = 0; j < vwp.EntitiesVisibleLast.Length; j++)
                {
                    found = vwp.EntitiesVisibleLast.Span[j].Id == b.Id;
                    if (found)
                        break;
                }

                if (found)
                    continue;
                
                ref readonly var bPhy = ref b.Get<PhysicsComponent>();
                ntt.NetSync(SpawnPacket.Create(b.Id, bPhy.ShapeType, bPhy.Radius, bPhy.Width, bPhy.Height, bPhy.Position, bPhy.RotationRadians, bPhy.Color));
            }
        }
    }
}