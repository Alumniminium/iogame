using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Effect, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EffectComponent(EffectType effectType, uint color = 0xFFFFFF)
{
    public long ChangedTick = NttWorld.Tick;

    public EffectType EffectType = effectType;
    public uint Color = color;
}
