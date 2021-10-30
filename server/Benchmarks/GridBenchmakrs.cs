using BenchmarkDotNet.Attributes;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Benchmarks
{
    [MemoryDiagnoser(true)]
    public class GridBenchmakrs
    {
        public static Entity Entity = new ();

        public GridBenchmakrs()
        {
            Entity.PositionComponent.Position = new (5000,5000);
        }

        [Benchmark]
        public void GetEntitiesViewport()
        {
            foreach(var x in CollisionDetection.Grid.GetEntitiesInViewport(Entity))
                x.BodyDamage--;
        }

        [Benchmark]
        public void GetEntitieSameCellAndSurrounding()
        {
            foreach (var x in CollisionDetection.Grid.GetEntitiesSameAndSurroundingCells(Entity))
                x.BodyDamage--;
        }
                [Benchmark]
        public void GetEntitiesSameAndSurroundingCellsList()
        {
            foreach (var x in CollisionDetection.Grid.GetEntitiesSameAndSurroundingCellsList(Entity))
                x.BodyDamage--;
        }

    }
}