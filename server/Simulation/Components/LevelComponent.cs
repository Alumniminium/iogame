using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct LevelComponent
    {
        public int Level;
        public int ExperienceToNextLevel;
        public int Experience;
        public uint ChangedTick;

        public LevelComponent(int level, int exp, int expReq)
        {
            Level = level;
            Experience = exp;
            ExperienceToNextLevel = expReq;
            ChangedTick = Game.CurrentTick;
        }
    }
}