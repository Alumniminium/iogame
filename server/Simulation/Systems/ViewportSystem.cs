using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{

    public sealed class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: Environment.ProcessorCount) { }
        override protected bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Pickable && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            if (phy.LastPosition == phy.Position && ntt.Type != EntityType.Player)
                return;

            vwp.Viewport.X = phy.Position.X - vwp.ViewDistance / 2;
            vwp.Viewport.Y = phy.Position.Y - vwp.ViewDistance / 2;

            vwp.EntitiesVisibleLast.Clear();
            vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
            vwp.EntitiesVisible.Clear();

            Game.Grid.GetVisibleEntities(in ntt);

            if(ntt.Type != EntityType.Player)
                return;

            for (var x = 0; x < vwp.EntitiesVisibleLast.Count; x++)
            {
                var visibleLast = vwp.EntitiesVisibleLast[x];

                if (!vwp.EntitiesVisible.Contains(visibleLast))
                    ntt.NetSync(StatusPacket.CreateDespawn(visibleLast.Id));
            }
            for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
            {
                var visibleLast = vwp.EntitiesVisible[x];

                if (!vwp.EntitiesVisibleLast.Contains(visibleLast))
                {
                    var addedEntity = vwp.EntitiesVisible[x];
                    if(addedEntity.Type == EntityType.Passive)
                        ntt.NetSync(ResourceSpawnPacket.Create(in addedEntity));
                    else
                        ntt.NetSync(SpawnPacket.Create(in addedEntity));
                }
            }
        }
    }
}