using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct EngineComponent
    {
        public float Throttle;
        public ushort MaxPropulsion;
        public bool RCS;

        public EngineComponent(ushort maxPropulsion, float throttle)
        {
            RCS = true;
            MaxPropulsion = maxPropulsion;
            Throttle = throttle;
        }
        public EngineComponent(ushort maxPropulsion)
        {
            RCS=true;
            MaxPropulsion = maxPropulsion;
            Throttle = 0;
        }
    }
}