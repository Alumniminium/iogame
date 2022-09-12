using System;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{

    public class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            if (Game.CurrentTick % 2 == 0)
                return;

            // if (phy.Position == phy.LastPosition)
            //     return;

            vwp.Viewport.X = phy.Position.X - vwp.ViewDistance / 2;
            vwp.Viewport.Y = phy.Position.Y - vwp.ViewDistance / 2;

            vwp.EntitiesVisibleLast.Clear();
            vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
            vwp.EntitiesVisible.Clear();

            vwp.EntitiesVisible = Game.Grid.GetEntitiesSameAndSurroundingCells(ntt);

            // for (var x = 0; x < vwp.EntitiesVisibleLast.Count; x++)
            // {
            //     var visibleLast = vwp.EntitiesVisibleLast[x];

            //     if (!vwp.EntitiesVisible.Contains(visibleLast) && ntt.IsPlayer())
            //     {
            //         ntt.NetSync(StatusPacket.CreateDespawn(visibleLast.Id));
            //     }
            // }
            for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
            {
                var visibleLast = vwp.EntitiesVisible[x];

                if (!vwp.EntitiesVisibleLast.Contains(visibleLast) && ntt.IsPlayer())
                {
                    var addedEntity = vwp.EntitiesVisible[x];
                    if (addedEntity.IsAsteroid())
                        ntt.NetSync(AsteroidSpawnPacket.Create(in addedEntity));
                    else if (addedEntity.IsFood())
                        ntt.NetSync(ResourceSpawnPacket.Create(in addedEntity));
                    else
                        ntt.NetSync(SpawnPacket.Create(in addedEntity));
                }
            }
        }
    }
}