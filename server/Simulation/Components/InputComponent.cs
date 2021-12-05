using System.Numerics;
using iogame.ECS;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iogame.Simulation.Components
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