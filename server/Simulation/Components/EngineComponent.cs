using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct EngineComponent
    {
        public readonly int EntityId;
        public float PowerUse;
        public float Throttle;
        public ushort MaxPropulsion;
        public bool RCS;
        public float Rotation;
        public uint ChangedTick;

        public EngineComponent(int entityId, ushort maxPropulsion)
        {
            EntityId = entityId;
            PowerUse = maxPropulsion * 2;
            RCS = true;
            MaxPropulsion = maxPropulsion;
            Throttle = 0;
            ChangedTick = Game.CurrentTick;
            Rotation = 0;
        }
        public override int GetHashCode() => EntityId;
    }
}