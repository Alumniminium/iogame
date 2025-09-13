using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct LevelComponent(int entityId, int level, int exp, int expReq)
{
    public readonly int EntityId = entityId;
    public int Level = level;
    public int ExperienceToNextLevel = expReq;
    public int Experience = exp;
    public uint ChangedTick = Game.CurrentTick;

    public override int GetHashCode() => EntityId;
}