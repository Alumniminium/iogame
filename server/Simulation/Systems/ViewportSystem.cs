using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{

    public sealed class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: 1) { }
        override protected bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Pickable && ntt.Type != EntityType.Static && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            if (phy.LastPosition == phy.Position && ntt.Type != EntityType.Player)
                return;

            vwp.EntitiesVisibleLast = new PixelEntity[vwp.EntitiesVisible.Length];
            Array.Copy(vwp.EntitiesVisible, vwp.EntitiesVisibleLast, vwp.EntitiesVisible.Length);

            Game.Grid.GetVisibleEntities(ref vwp);

            if(ntt.Type != EntityType.Player)
                return;

            for (var i = 0; i < vwp.EntitiesVisibleLast.Length; i++)
            {
                var b = vwp.EntitiesVisibleLast[i];
                var found = false;

                for (var j = 0; j < vwp.EntitiesVisible.Length; j++)
                {
                    if (vwp.EntitiesVisible[j].Id == b.Id)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    ntt.NetSync(StatusPacket.CreateDespawn(b.Id));
            }

            for (var i = 0; i < vwp.EntitiesVisible.Length; i++)
            {
                var b = vwp.EntitiesVisible[i];
                var found = false;

                for (var j = 0; j < vwp.EntitiesVisibleLast.Length; j++)
                {
                    if (vwp.EntitiesVisibleLast[j].Id == b.Id)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    ntt.NetSync(SpawnPacket.Create(b));
            }
        }
    }
}