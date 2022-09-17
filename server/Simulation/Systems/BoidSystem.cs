using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class BoidSystem : PixelSystem<PhysicsComponent, InputComponent, BoidComponent, ViewportComponent>
    {
        public BoidSystem() : base("BoidSystem System", threads: 1) { }
        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref InputComponent inp, ref BoidComponent boi, ref ViewportComponent vwp)
        {

        }
    }
}