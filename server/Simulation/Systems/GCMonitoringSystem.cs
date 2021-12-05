using System;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Systems
{
    public class GcMonitor : PixelSystem
    {
        public int[] GenCollections = new int[3];
        public DateTime LastUpdate = DateTime.UtcNow;
        public GcMonitor() :base("GC Monitoring System") { }

        public override bool MatchesFilter(ref PixelEntity entityId) => false;

        public override void Update(float dt, RefList<PixelEntity> entity)
        {
            if (DateTime.UtcNow >= LastUpdate.AddSeconds(1))
            {
                LastUpdate = DateTime.UtcNow;
                for (int i = 0; i < GenCollections.Length; i++)
                {
                    var newVal = GC.CollectionCount(i);
                    var oldVal = GenCollections[i];

                    if (newVal != oldVal)
                    {
                        FConsole.WriteLine($"GC: Gen0: {GenCollections[0]:000}, Gen1: {GenCollections[1]:000}, Gen2: {GenCollections[2]:000}");
                        GenCollections[i] = newVal;
                    }
                }
            }
        }
    }
}