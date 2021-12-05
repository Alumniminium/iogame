using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct InputComponent
    {
        public Vector2 MovementAxis;
        public Vector2 MousePositionWorld;
        public bool Fire;
        public uint LastShot;
    }
}