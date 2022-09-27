using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct LevelComponent
    {
        public readonly int EntityId;
        public int Level;
        public int ExperienceToNextLevel;
        public int Experience;
        public uint ChangedTick;

        public LevelComponent(int entityId, int level, int exp, int expReq)
        {
            EntityId = entityId;
            Level = level;
            Experience = exp;
            ExperienceToNextLevel = expReq;
            ChangedTick = Game.CurrentTick;
        }
        public override int GetHashCode() => EntityId;
    }
}