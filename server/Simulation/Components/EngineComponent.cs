using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct EngineComponent
    {
        public Vector2 Propulsion;
        public ushort MaxPropulsion;

        public EngineComponent(ushort maxPropulsion, Vector2 initialPropulsion)
        {
            MaxPropulsion = maxPropulsion;
            Propulsion = initialPropulsion;
        }
        public EngineComponent(ushort maxPropulsion)
        {
            MaxPropulsion = maxPropulsion;
            Propulsion = Vector2.Zero;
        }
    }
}