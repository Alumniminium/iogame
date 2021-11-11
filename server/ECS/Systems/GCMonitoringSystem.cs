using iogame.ECS;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class GCMonitor : PixelSystem
    {
        public int[] GenCollections = new int[3];
        public DateTime LastUpdate = DateTime.UtcNow;
        public GCMonitor()
        {
            Name = "GC Monitoring System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<Entity> entities)
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