using System;
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
        public uint ChangedTick;
        public float Rotation;

        public EngineComponent(ushort maxPropulsion, float throttle)
        {
            RCS = true;
            MaxPropulsion = maxPropulsion;
            Throttle = throttle;
            ChangedTick = Game.CurrentTick;
            Rotation = 0;
        }
        public EngineComponent(ushort maxPropulsion)
        {
            RCS = true;
            MaxPropulsion = maxPropulsion;
            Throttle = 0;
            ChangedTick = Game.CurrentTick;
            Rotation = 0;
        }
    }
}