using iogame.Simulation.Components;
using iogame.Simulation.Managers;

namespace iogame.Simulation.Entities
{
    public unsafe class Boid : ShapeEntity
    {
        public ref BoidComponent BoidComponent => ref Entity.Get<BoidComponent>();
        public ref InputComponent InputComponent => ref Entity.Get<InputComponent>();

        public Boid()
        {
        }
    }
}