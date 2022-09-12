using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{

    public class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: Environment.ProcessorCount) { }

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

            for (var x = 0; x < vwp.EntitiesVisibleLast.Count; x++)
            {
                var visibleLast = vwp.EntitiesVisibleLast[x];

                if (!vwp.EntitiesVisible.Contains(visibleLast) && ntt.Type == EntityType.Player)
                {
                    ntt.NetSync(StatusPacket.CreateDespawn(visibleLast.Id));
                }
            }
            for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
            {
                var visibleLast = vwp.EntitiesVisible[x];

                if (!vwp.EntitiesVisibleLast.Contains(visibleLast) && ntt.Type == EntityType.Player)
                {
                    var addedEntity = vwp.EntitiesVisible[x];
                    if (addedEntity.Type == EntityType.Asteroid)
                        ntt.NetSync(AsteroidSpawnPacket.Create(in addedEntity));
                    else if (addedEntity.Type == EntityType.Food)
                        ntt.NetSync(ResourceSpawnPacket.Create(in addedEntity));
                    else
                        ntt.NetSync(SpawnPacket.Create(in addedEntity));
                }
            }
        }
    }
}