using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct EngineComponent
    {
        public float PowerUse;
        public float Throttle;
        public ushort MaxPropulsion;
        public bool RCS;
        public float Rotation;
        public uint ChangedTick;

        public EngineComponent(ushort maxPropulsion)
        {
            PowerUse = maxPropulsion;
            RCS = true;
            MaxPropulsion = maxPropulsion;
            Throttle = 0;
            ChangedTick = Game.CurrentTick;
            Rotation = 0;
        }
    }
}