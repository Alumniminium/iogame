using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct LevelComponent(NTT EntityId, int level, int exp, int expReq)
{
    public readonly NTT EntityId = EntityId;
    public int Level = level;
    public int ExperienceToNextLevel = expReq;
    public int Experience = exp;
    public long ChangedTick = NttWorld.Tick;


}