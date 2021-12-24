using server.ECS;
using server.Helpers;

namespace server.Simulation.Systems
{
    public class GcMonitor : PixelSystem
    {
        private readonly int[] _genCollections = new int[3];
        public GcMonitor() :base("GC Monitoring System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity entityId) => false;

        protected override void Update(float dt, List<PixelEntity> entity)
        {
            for (var i = 0; i < _genCollections.Length; i++)
            {
                var newVal = GC.CollectionCount(i);
                var oldVal = _genCollections[i];

                if (newVal != oldVal)
                {
                    FConsole.WriteLine($"GC: Gen0: {_genCollections[0]:000}, Gen1: {_genCollections[1]:000}, Gen2: {_genCollections[2]:000}");
                    _genCollections[i] = newVal;
                }
            }
        }
    }
}