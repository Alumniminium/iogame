using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Level, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LevelComponent(int level, int exp, int expReq)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public int Level = level;
    public int ExperienceToNextLevel = expReq;
    public int Experience = exp;
}