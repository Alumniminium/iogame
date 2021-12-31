using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct EngineComponent
    {
        public Vector2 Propulsion;
        public ushort MaxPropulsion;
        public bool RCS;

        public EngineComponent(ushort maxPropulsion, Vector2 initialPropulsion)
        {
            RCS = true;
            MaxPropulsion = maxPropulsion;
            Propulsion = initialPropulsion;
        }
        public EngineComponent(ushort maxPropulsion)
        {
            RCS=true;
            MaxPropulsion = maxPropulsion;
            Propulsion = Vector2.Zero;
        }
    }
}