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

        public InputComponent(Vector2 movement, Vector2 mousePos, bool fire, uint lastShotTick)
        {
            MovementAxis = movement;
            MousePositionWorld = mousePos;
            Fire = fire;
            LastShot = lastShotTick;
        }
    }
}